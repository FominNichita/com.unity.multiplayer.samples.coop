using BossRoom.Client;
using Cinemachine;
using MLAPI;
using System;
using UnityEngine;

namespace BossRoom.Visual
{
    /// <summary>
    /// <see cref="ClientCharacterVisualization"/> is responsible for displaying a character on the client's screen based on state information sent by the server.
    /// </summary>
    public class ClientCharacterVisualization : NetworkedBehaviour
    {
        private NetworkCharacterState m_NetState;

        [SerializeField]
        private Animator m_ClientVisualsAnimator;

        [SerializeField]
        private CharacterSwap m_CharacterSwapper;

        [Tooltip("Prefab for the Target Reticule used by this Character")]
        public GameObject TargetReticule;

        [Tooltip("Material to use when displaying a friendly target reticule (e.g. green color)")]
        public Material ReticuleFriendlyMat;

        [Tooltip("Material to use when displaying a hostile target reticule (e.g. red color)")]
        public Material ReticuleHostileMat;

        public Animator OurAnimator { get { return m_ClientVisualsAnimator; } }

        private ActionVisualization m_ActionViz;

        private CinemachineVirtualCamera m_MainCamera;

        public Transform Parent { get; private set; }

        public float MinZoomDistance = 3;
        public float MaxZoomDistance = 30;
        public float ZoomSpeed = 3;

        private const float k_MaxRotSpeed = 280;  //max angular speed at which we will rotate, in degrees/second.

        /// Player characters need to report health changes and chracter info to the PartyHUD
        private Visual.PartyHUD m_PartyHUD;

        private float m_SmoothedSpeed;

        /// <inheritdoc />
        public override void NetworkStart()
        {
            if (!IsClient || transform.parent == null)
            {
                enabled = false;
                return;
            }

            m_ActionViz = new ActionVisualization(this);

            m_NetState = transform.parent.gameObject.GetComponent<NetworkCharacterState>();
            m_NetState.DoActionEventClient += PerformActionFX;
            m_NetState.CancelActionEventClient += CancelActionFX;
            m_NetState.NetworkLifeState.OnValueChanged += OnLifeStateChanged;
            m_NetState.OnPerformHitReaction += OnPerformHitReaction;
            m_NetState.OnStopChargingUpClient += OnStoppedChargingUp;
            // With this call, players connecting to a game with down imps will see all of them do the "dying" animation.
            // we should investigate for a way to have the imps already appear as down when connecting.
            // todo gomps-220
            OnLifeStateChanged(m_NetState.NetworkLifeState.Value, m_NetState.NetworkLifeState.Value);

            //we want to follow our parent on a spring, which means it can't be directly in the transform hierarchy.
            Parent = transform.parent;
            Parent.GetComponent<Client.ClientCharacter>().ChildVizObject = this;
            transform.parent = null;

            // listen for char-select info to change (in practice, this info doesn't
            // change, but we may not have the values set yet) ...
            m_NetState.CharacterAppearance.OnValueChanged += OnCharacterAppearanceChanged;

            // ...and visualize the current char-select value that we know about
            OnCharacterAppearanceChanged(0, m_NetState.CharacterAppearance.Value);


            // ...and visualize the current char-select value that we know about
            if (m_CharacterSwapper)
            {
                m_CharacterSwapper.SwapToModel(m_NetState.CharacterAppearance.Value);
            }

            if (!m_NetState.IsNpc)
            {
                // track health for heroes
                m_NetState.HealthState.HitPoints.OnValueChanged += OnHealthChanged;

                Client.CharacterSwap model = GetComponent<Client.CharacterSwap>();
                int heroAppearance = m_NetState.CharacterAppearance.Value;
                model.SwapToModel(heroAppearance);

                // find the emote bar to track its buttons
                GameObject partyHUDobj = GameObject.FindGameObjectWithTag("PartyHUD");
                m_PartyHUD = partyHUDobj.GetComponent<Visual.PartyHUD>();

                if (IsLocalPlayer)
                {
                    ActionRequestData data = new ActionRequestData { ActionTypeEnum = ActionType.GeneralTarget };
                    m_ActionViz.PlayAction(ref data);
                    AttachCamera();
                    m_PartyHUD.SetHeroAppearance(heroAppearance);
                    m_PartyHUD.SetHeroType(m_NetState.CharacterType);
                }
                else
                {
                    m_PartyHUD.SetAllyType(m_NetState.NetworkId, m_NetState.CharacterType);
                }

            }
        }

        private void OnDestroy()
        {
            if (m_NetState)
            {
                m_NetState.DoActionEventClient -= PerformActionFX;
                m_NetState.CancelActionEventClient -= CancelActionFX;
                m_NetState.NetworkLifeState.OnValueChanged -= OnLifeStateChanged;
                m_NetState.OnPerformHitReaction -= OnPerformHitReaction;
                m_NetState.OnStopChargingUpClient -= OnStoppedChargingUp;
            }

            if (m_ActionViz != null)
            {
                //make sure we don't leave any dangling effects playing if we've been destroyed. 
                m_ActionViz.CancelAll();
            }
        }

        private void OnPerformHitReaction()
        {
            m_ClientVisualsAnimator.SetTrigger("HitReact1");
        }

        private void PerformActionFX(ActionRequestData data)
        {
            m_ActionViz.PlayAction(ref data);
        }

        private void CancelActionFX()
        {
            m_ActionViz.CancelActions();
        }

        private void OnStoppedChargingUp()
        {
            m_ActionViz.OnStoppedChargingUp();
        }

        private void OnLifeStateChanged(LifeState previousValue, LifeState newValue)
        {
            switch (newValue)
            {
                case LifeState.Alive:
                    m_ClientVisualsAnimator.SetTrigger("StandUp");
                    break;
                case LifeState.Fainted:
                    m_ClientVisualsAnimator.SetTrigger("FallDown");
                    break;
                case LifeState.Dead:
                    m_ClientVisualsAnimator.SetTrigger("Dead");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newValue), newValue, null);
            }
        }

        private void OnHealthChanged(int previousValue, int newValue)
        {
            if (IsLocalPlayer)
            {
                this.m_PartyHUD.SetHeroHealth(newValue);
            }
            else
            {
                this.m_PartyHUD.SetAllyHealth(m_NetState.NetworkId, newValue);
            }
        }

        private void OnCharacterAppearanceChanged(int oldValue, int newValue)
        {
            if (m_CharacterSwapper)
            {
                m_CharacterSwapper.SwapToModel(m_NetState.CharacterAppearance.Value);
            }
        }

        void Update()
        {
            if (Parent == null)
            {
                // since we aren't in the transform hierarchy, we have to explicitly die when our parent dies.
                Destroy(gameObject);
                return;
            }

            VisualUtils.SmoothMove(transform, Parent.transform, Time.deltaTime, ref m_SmoothedSpeed, k_MaxRotSpeed);

            if (m_ClientVisualsAnimator)
            {
                // set Animator variables here
                m_ClientVisualsAnimator.SetFloat("Speed", m_NetState.VisualMovementSpeed.Value);
            }

            m_ActionViz.Update();

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0 && m_MainCamera)
            {
                ZoomCamera(scroll);
            }

        }

        public void OnAnimEvent(string id)
        {
            //if you are trying to figure out who calls this method, it's "magic". The Unity Animation Event system takes method names as strings,
            //and calls a method of the same name on a component on the same GameObject as the Animator. See the "attack1" Animation Clip as one
            //example of where this is configured.

            m_ActionViz.OnAnimEvent(id);
        }

        private void AttachCamera()
        {
            var cameraGO = GameObject.FindGameObjectWithTag("CMCamera");
            if (cameraGO == null) { return; }

            m_MainCamera = cameraGO.GetComponent<CinemachineVirtualCamera>();
            if (m_MainCamera)
            {
                m_MainCamera.Follow = transform;
                m_MainCamera.LookAt = transform;
            }
        }

        private void ZoomCamera(float scroll)
        {
            CinemachineComponentBase[] components = m_MainCamera.GetComponentPipeline();
            foreach (CinemachineComponentBase component in components)
            {
                if (component is CinemachineFramingTransposer)
                {
                    CinemachineFramingTransposer c = (CinemachineFramingTransposer)component;
                    c.m_CameraDistance += -scroll * ZoomSpeed;
                    if (c.m_CameraDistance < MinZoomDistance)
                        c.m_CameraDistance = MinZoomDistance;
                    if (c.m_CameraDistance > MaxZoomDistance)
                        c.m_CameraDistance = MaxZoomDistance;
                }
            }
        }
    }
}
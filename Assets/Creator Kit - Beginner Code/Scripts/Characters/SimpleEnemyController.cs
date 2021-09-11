using CreatorKitCode;
using UnityEngine;
using UnityEngine.AI;

namespace CreatorKitCodeInternal {
    public class SimpleEnemyController : MonoBehaviour, 
        AnimationControllerDispatcher.IAttackFrameReceiver,
        AnimationControllerDispatcher.IFootstepFrameReceiver
    {
        public enum State
        {
            IDLE,
            PURSUING,
            ATTACKING
        }
    
        public float Speed = 6.0f;
        public float detectionRadius = 10.0f;

        public AudioClip[] SpottedAudioClip;

        Vector3 m_StartingAnchor;
        Animator m_Animator;
        NavMeshAgent m_Agent;
        CharacterData m_CharacterData;

        CharacterAudio m_CharacterAudio;

        int m_SpeedAnimHash;
        int m_AttackAnimHash;
        int m_DeathAnimHash;
        int m_HitAnimHash;
        bool m_Pursuing;
        float m_PursuitTimer = 0.0f;

        State m_State;

        LootSpawner m_LootSpawner;
    
        // Start is called before the first frame update
        void Start()
        {
            m_Animator = GetComponentInChildren<Animator>();
            m_Agent = GetComponent<NavMeshAgent>();
        
            m_SpeedAnimHash = Animator.StringToHash("Speed");
            m_AttackAnimHash = Animator.StringToHash("Attack");
            m_DeathAnimHash = Animator.StringToHash("Death");
            m_HitAnimHash = Animator.StringToHash("Hit");

            m_CharacterData = GetComponent<CharacterData>();
            m_CharacterData.Init();

            m_CharacterAudio = GetComponentInChildren<CharacterAudio>();
        
            m_CharacterData.OnDamage += () =>
            {
                m_Animator.SetTrigger(m_HitAnimHash);
                m_CharacterAudio.Hit(transform.position);
            };
        
            m_Agent.speed = Speed;

            m_LootSpawner = GetComponent<LootSpawner>();
        
            m_StartingAnchor = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            //See the Update function of CharacterControl.cs for a comment on how we could replace
            //this (polling health) to a callback method.
            if (m_CharacterData.Stats.CurrentHealth == 0)
            {
                m_Animator.SetTrigger(m_DeathAnimHash);
            
                m_CharacterAudio.Death(transform.position);
                m_CharacterData.Death();
            
                if(m_LootSpawner != null)
                    m_LootSpawner.SpawnLoot();
            
                Destroy(m_Agent);
                Destroy(GetComponent<Collider>());
                Destroy(this);
                return;
            }
        
            //NOTE : in a full game, this would use a targetting system that would give the closest target
            //of the opposing team (e.g. multiplayer or player owned pets). Here for simplicity we just reference
            //directly the player.
            Vector3 playerPosition = CharacterControl.Instance.transform.position;
            CharacterData playerData = CharacterControl.Instance.Data;
        
            switch (m_State)
            {
                case State.IDLE:
                {
                    if (Vector3.SqrMagnitude(playerPosition - transform.position) < detectionRadius * detectionRadius)
                    {
                        if (SpottedAudioClip.Length != 0)
                        {
                            SFXManager.PlaySound(SFXManager.Use.Enemies, new SFXManager.PlayData()
                            {
                                Clip = SpottedAudioClip[Random.Range(0, SpottedAudioClip.Length)],
                                Position = transform.position
                            });
                        }

                        m_PursuitTimer = 4.0f;
                        m_State = State.PURSUING;
                        m_Agent.isStopped = false;
                    }
                }
                    break;
                case State.PURSUING:
                {
                    float distToPlayer = Vector3.SqrMagnitude(playerPosition - transform.position);
                    if (distToPlayer < detectionRadius * detectionRadius)
                    {
                        m_PursuitTimer = 4.0f;

                        if (m_CharacterData.CanAttackTarget(playerData))
                        {
                            m_CharacterData.AttackTriggered();
                            m_Animator.SetTrigger(m_AttackAnimHash);
                            m_State = State.ATTACKING;
                            m_Agent.ResetPath();
                            m_Agent.velocity = Vector3.zero;
                            m_Agent.isStopped = true;
                        }
                    }
                    else
                    {
                        if (m_PursuitTimer > 0.0f)
                        {
                            m_PursuitTimer -= Time.deltaTime;

                            if (m_PursuitTimer <= 0.0f)
                            {
                                m_Agent.SetDestination(m_StartingAnchor);
                                m_State = State.IDLE;
                            }
                        }
                    }
                
                    if (m_PursuitTimer > 0)
                    {
                        m_Agent.SetDestination(playerPosition);
                    }
                }
                    break;
                case State.ATTACKING:
                {
                    if (!m_CharacterData.CanAttackReach(playerData))
                    {
                        m_State = State.PURSUING;
                        m_Agent.isStopped = false;
                    }
                    else
                    {
                        if (m_CharacterData.CanAttackTarget(playerData))
                        {
                            m_CharacterData.AttackTriggered();
                            m_Animator.SetTrigger(m_AttackAnimHash);
                        }
                    }
                }
                    break;
            }
        
            m_Animator.SetFloat(m_SpeedAnimHash, m_Agent.velocity.magnitude/Speed);
        }

        public void AttackFrame()
        {
            CharacterData playerData = CharacterControl.Instance.Data;
            
            //if we can't reach the player anymore when it's time to damage, then that attack miss.
            if (!m_CharacterData.CanAttackReach(playerData))
                return;
            
            m_CharacterData.Attack(playerData);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }

        public void FootstepFrame()
        {
            Vector3 pos = transform.position;
        
            m_CharacterAudio.Step(pos);
            VFXManager.PlayVFX(VFXType.StepPuff, pos); 
        }
    }
}
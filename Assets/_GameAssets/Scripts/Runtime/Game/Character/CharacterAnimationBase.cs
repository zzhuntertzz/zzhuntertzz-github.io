using Lean.Pool;
using UnityEngine;

    public class CharacterAnimationBase : MonoBehaviour,
        IAnimResetable, IAnimDie, IAnimMove, IAnimAttack, IAnimHurt,
        IPoolable
    {
        protected Animator _animator;

        protected virtual void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
        }

        public void OnSpawn()
        {
            var scale = transform.localScale.y;
            transform.localScale = new(
                (FunctionCommon.Random(1, 100) > 50 ? 1 : -1) * scale, scale);
        }

        public void OnDespawn()
        {
        }

        private void OnEnable()
        {
            if (!_animator) return;
            ResetAnimation();
        }

        public virtual void ResetAnimation()
        {
            if (!_animator) return;
            _animator.Rebind();
            _animator.Update(0f);
            _animator.updateMode = AnimatorUpdateMode.Normal;
        }
        
        public virtual void Died()
        {
            if (!_animator) return;
            ResetAnimation();
            _animator.SetTrigger(nameof(Died));
        }
        
        public virtual void Died2()
        {
            if (!_animator) return;
            _animator.Update(0f);
            ResetAnimation();
            _animator.SetTrigger(nameof(Died2));
        }

        public virtual void SetAnimMove(float dirX)
        {
            if (!_animator) return;
            _animator.SetBool("Move", dirX != 0);
        }

        public void DoAttack()
        {
            if (!_animator) return;
            _animator.Update(0f);
            ResetAnimation();
            _animator.SetTrigger("Attack");
        }

        public void SetAttackSpeed(float spd)
        {
            _animator.SetFloat("atkSpd", spd);
        }

        public void SetHurtState(bool isHurt)
        {
            // _animator.SetBool("Hurt", isHurt);
        }
    }

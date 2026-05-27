using UnityEngine;

namespace Abdulrahman.EnemySystem
{
    public class ZombieAI : BaseEnemyAI
    {
        protected override void HandleAttack(float distance)
        {
            if (_attackTimer <= 0f && !_isAttacking)
            {
                _attackTimer = attackCooldown;
                _isAttacking = true;

                int randomAttack = Random.Range(0, 2);
                if (randomAttack == 0)
                    _animator.SetTrigger("Attack1Trigger");
                else
                    _animator.SetTrigger("Attack2Trigger");
            }

            if (_isAttacking)
            {
                AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
                bool attackDone = (stateInfo.IsName("Attack1") || stateInfo.IsName("Attack2"))
                                  && stateInfo.normalizedTime >= 0.9f;

                if (attackDone)
                    _isAttacking = false;
            }
        }
    }
}
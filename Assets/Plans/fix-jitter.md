# Fix Movement Jitter

Analyze and resolve movement/camera jitter caused by script conflicts and improper update loop synchronization.

## Steps
1. Analyze existing scripts for movement and rotation conflicts.
2. Modify StableFirstPersonCamera to process input and root rotation in Update to prevent 1-frame lag.
3. Optimize PlayerController to ensure single CharacterController.Move call and sane knockback values.
4. Perform scene cleanup: remove redundant scripts and check for root motion/collider conflicts.
5. Run Play Mode test to verify fix.
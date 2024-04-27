using UnityEngine;

public class HeroAnimationController : AnimationController
{
  protected override void SetLocomotionClip()
  {
    locomotionClip = Animator.StringToHash("MoveFWD_Normal_RM_SwordAndShield");
  }

  protected override void SetAttackClip()
  {
    //attackClip = Animator.StringToHash("Attack01_MagicWand");
    attackClip = Animator.StringToHash("Attack01_SwordAndShiled");
  }

  protected override void SetSpeedHash()
  {
    speedHash = Animator.StringToHash("Speed");
  }
}
using UnityEngine;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;

/// <summary>
/// To draw attack animation with player.
/// </summary>
public class PcControl : MonoBehaviour {

    public GameSystem gameSystem;

    public GameObject slashEffect;
    public GameObject bloodEffect;
    public Slider hpBar;
    public SpriteRenderer idleSprite, attackSprite, damageSprite,elementSprite,activeBackground, freezeStatus;
    SpriteRenderer sRender;

    public SpriteRenderer pcAntenna, pcHead, pcEyes, pcBody, pcRightArm, pcLeftArm, pcLegs;

    float healthPoint = 1f;

    Animator animator;

    public Data.TileTypes baseElement;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        sRender = GetComponent<SpriteRenderer>();
        freezeStatus.enabled = false;
    }

	IEnumerator DoneAttack(float delayTime) {
		yield return new WaitForSeconds(delayTime);
        if (idleSprite) idleSprite.enabled = false;
		if (attackSprite) attackSprite.enabled = false;
        if (animator) animator.CrossFade("Idle", 0.2f);
    }

    IEnumerator DoneDamage(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        if (idleSprite) idleSprite.enabled = false;
        if (damageSprite) damageSprite.enabled = false;
        if (animator) animator.CrossFade("Idle", 0.2f);
    }

	IEnumerator DoAttack(float delayTime) {
        if (idleSprite) idleSprite.enabled = false;
        if (attackSprite) attackSprite.enabled = false;
		yield return new WaitForSeconds(delayTime);
        GameObject instance = Instantiate(slashEffect) as GameObject;
        instance.transform.parent = transform.parent;
        instance.transform.localPosition = new Vector3(0f, 80f, 0f);
    }

    IEnumerator DoDamage(float delayTime)
    {
        //if (damageSprite) damageSprite.enabled = true;
        if (idleSprite) idleSprite.enabled = false;
        yield return new WaitForSeconds(delayTime);
        GameObject instance = Instantiate(bloodEffect) as GameObject;
        instance.transform.parent = transform.parent;
        instance.transform.localRotation = Quaternion.Euler(-45f, -140f, 0f); ;
        if (sRender)
            instance.transform.localPosition = transform.localPosition + new Vector3(0f, 20f, 0f);
        else
            instance.transform.localPosition = transform.localPosition + new Vector3(0f, 100f, 0f);
    }

	public void SetHealthPoint(float point){
		if (point<0f) point = 0f;
		if (point>1f) point = 1f;
		TweenParms parms = new TweenParms().Prop("sliderValue", point).Ease(EaseType.EaseOutQuart);
		HOTween.To(hpBar, 0.1f, parms );
		healthPoint = point;
	}

    public float GetHealthPoint()
    {
        return healthPoint;
    }

	void SetHealthDamage(float damage){
		SetHealthPoint(healthPoint - damage);
	}

    public void SetHealthUp(float heal)
    {
        SetHealthPoint(healthPoint + heal);
    }

    public void Attack(){
        if (animator) animator.CrossFade("Attack", 0.2f);
        StartCoroutine(DoAttack(0.5f));
		StartCoroutine( DoneAttack(0.5f) );
        
    }
    public void Damage(float damage)
    {
        if (animator) animator.CrossFade("Damage", 0.2f);
        StartCoroutine(DoDamage(0.1f));
        StartCoroutine(DoneDamage(0.1f));
        SetHealthDamage(damage);
    }

    public void SetElement(int randomElement)
    {
        baseElement = (Data.TileTypes)randomElement + 1;
    }

    public Data.TileTypes GetElement()
    {
        return baseElement;
    }

    public void EnableActiveBackground(bool enable)
    {
        activeBackground.enabled = enable;
    }

    public void EnableFreezeStatus(bool enable)
    {
        freezeStatus.enabled = enable;
    }

    public void SetPlayerRobotPartsFromAvatar()
    {
        pcAntenna.sprite = gameSystem.GetRobotAntennas()[GameState.control.antennaIndex];
        pcHead.sprite = gameSystem.GetRobotHeads()[GameState.control.headIndex];
        pcEyes.sprite = gameSystem.GetRobotEyes()[GameState.control.eyesIndex];
        pcBody.sprite = gameSystem.GetRobotBodies()[GameState.control.bodyIndex];
        pcRightArm.sprite = gameSystem.GetRobotRightArms()[GameState.control.rightArmIndex];
        pcLeftArm.sprite = gameSystem.GetRobotLeftArms()[GameState.control.leftArmIndex];
        pcLegs.sprite = gameSystem.GetRobotLegs()[GameState.control.legIndex];
    }
}

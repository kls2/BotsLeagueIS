using UnityEngine;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;

/// <summary>
/// To draw damage motion with npc monster.
/// </summary>
public class NpcControl : MonoBehaviour {

    public GameSystem gameSystem;

    public GameObject slashEffect;
    public GameObject bloodEffect;
    public Slider hpBar;
    public SpriteRenderer idleSprite, attackSprite, damageSprite,elementSprite,activeBackground, freezeStatus;

    public SpriteRenderer npcAntenna, npcHead, npcEyes, npcBody, npcRightArm, npcLeftArm, npcLegs;

    SpriteRenderer sRender;
	
	float healthPoint = 1f;

    public Element element;

    public Data.TileTypes baseElement;


    public int health;
    public int maxHealth;



    Animator animator;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        sRender = GetComponent<SpriteRenderer>();
        freezeStatus.enabled = false;
    }

    IEnumerator DoneAttack(float delayTime)
    {
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

    IEnumerator DoAttack(float delayTime)
    {
        if (idleSprite) idleSprite.enabled = false;
        if (attackSprite) attackSprite.enabled = false;
        yield return new WaitForSeconds(delayTime);
        GameObject instance = Instantiate(slashEffect) as GameObject;
        instance.transform.parent = transform.parent;
        instance.transform.localPosition = new Vector3(0f, 80f, 0f);
    }

	IEnumerator DoDamage(float delayTime) {
        if (damageSprite) damageSprite.enabled = false;
        if (idleSprite) idleSprite.enabled = false;
		yield return new WaitForSeconds(delayTime);
        GameObject instance = Instantiate(bloodEffect) as GameObject;
        instance.transform.parent = transform.parent;
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

	void SetHealthDamage(float damage){
		SetHealthPoint(healthPoint - damage);
	}

    public void SetHealthUp(float heal)
    {
        SetHealthPoint(healthPoint + heal);
    }

    public void Damage(float damage){
        if (animator) animator.CrossFade("Damage", 0.2f);
        StartCoroutine(DoDamage(0.1f));
        StartCoroutine(DoneDamage(0.1f));
        SetHealthDamage(damage);
    }

    void Die()
    {
        // Probably destroy the game object or something
    }

    public float GetHealthPoint()
    {
        return healthPoint;
    }

    public void Attack()
    {
        if (animator) animator.CrossFade("Attack", 0.2f);
        StartCoroutine(DoAttack(0.5f));
        StartCoroutine(DoneAttack(0.5f));
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

    public void RandomizeEnemyParts()
    {
        int rand = Random.Range(0, gameSystem.GetRobotAntennas().Length);
        npcAntenna.sprite = gameSystem.GetRobotAntennas()[rand];
        rand = Random.Range(0, gameSystem.GetRobotHeads().Length);
        npcHead.sprite = gameSystem.GetRobotHeads()[rand];
        rand = Random.Range(0, gameSystem.GetRobotEyes().Length);
        npcEyes.sprite = gameSystem.GetRobotEyes()[rand];
        rand = Random.Range(0, gameSystem.GetRobotBodies().Length);
        npcBody.sprite = gameSystem.GetRobotBodies()[rand];
        rand = Random.Range(0, gameSystem.GetRobotRightArms().Length);
        npcRightArm.sprite = gameSystem.GetRobotRightArms()[rand];
        rand = Random.Range(0, gameSystem.GetRobotLeftArms().Length);
        npcLeftArm.sprite = gameSystem.GetRobotLeftArms()[rand];
        rand = Random.Range(0, gameSystem.GetRobotLegs().Length);
        npcLegs.sprite = gameSystem.GetRobotLegs()[rand];
    }
}


public enum Element
{
    EMPTY = 0,
    ICE = 1,
    FIRE = 2,
    WATER = 3,
    MAGNET = 4,
    LIFE = 5,
    ELEC = 6,
    LENGTH = 6,
}
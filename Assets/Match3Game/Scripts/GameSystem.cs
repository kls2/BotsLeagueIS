using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;
using UnityEngine.SceneManagement;

// To record the location of data.
public struct TilePoint {
	public int x, y;
	public TilePoint(int px, int py){
		x = px;
		y = py;
	}
	public override string ToString() {
		 return "(" + x + ", " + y + ")";
	}
}

// Default setting and type of tile.
public class Data {
	public const int tileWidth = 7;
	public const int tileHeight = 7;
	public enum TileTypes  { Empty, White, Red, Blue, Magnet, Green, Yellow, Length };
}

// To record the location of tile data.
[System.Serializable]
public class Cell {
	public Data.TileTypes cellType;
	public bool IsEmpty { get { return cellType == Data.TileTypes.Empty; } }
	public void SetRandomTile(int total) {
		cellType = (Data.TileTypes) UnityEngine.Random.Range(0, total) + 1;
	}
}

// Main Game System (and hopefully it will work)
public class GameSystem : MonoBehaviour {

    //Setting the pieces types (sprite and audio) that will fill the grid
    public Sprite[] sprites; //{"WhiteIce", "RedFire", "BlueWater", "MagnetIron", "GreenLife", "YellowElectricity"};
    public Sprite[] spritesSelected; //{"WhiteIce", "RedFire", "BlueWater", "MagnetIron", "GreenLife", "YellowElectricity"};
    string[] sounds = new string[]{"PieceWhiteIceAudio", "PieceRedFireAudio", "PieceBlueWaterAudio", "PieceMagnetIronAudio", "PieceGreenLifeAudio", "PieceYellowElectricityAudio"};
    public EaseType easeType = EaseType.EaseOutBounce;
	
	public const int cellScale = 120;
	public const int cellWidth = 108;
	public const int cellHeight = 108;

	public GameObject grid, puzzle, panel;
	public GameObject matchItemPrefab;
	public GameObject explosionPrefab;
	public Transform effectArea;
	
	public GameObject[] starEffectPrefabs;
   
	public SpriteRenderer choice;
	List<MatchItem> tiles;
	private Cell[,] cells = new Cell[Data.tileWidth, Data.tileHeight];
	public MatchItem curTile = null;

	public AudioClip[] audioMatchClip = null;
	public AudioSource[] audioMatchSource = null;

	public PcControl pcControl;  
	public NpcControl npcControl;
    public Sprite[] deleteSprites;

	bool isDoing = false;

    public bool canDoInput;

    public int playerCount = 2;

    //Who's turn is it
    public bool playerHumanTurn = true;
    public bool wasMatch3 = false;

    public float healValue = 0.2f;

    private int bonusTurns = 0;
    private int lastStackCount = 0;
    private bool nullifyAttack = false;

	// Setup Audio Source - fuck the bar that took me 5h.
	void SetupAudioSource(){
		audioMatchClip = new AudioClip[sounds.Length];
		audioMatchSource = new AudioSource[sounds.Length];
		for (int i=0; i<sounds.Length; i++) {
			audioMatchClip[i] = Resources.Load("Sounds/" + sounds[i], typeof(AudioClip)) as AudioClip;
			audioMatchSource[i] = gameObject.AddComponent<AudioSource>();
			audioMatchSource[i].clip = audioMatchClip[i];
		}
	}

	// Init Tile Grid
	public void InitTileGrid() {
		for (int x = 0; x < Data.tileWidth; x++) {
			for (var y = 0; y < Data.tileHeight; y++) {
				cells[x, y] = new Cell();
				cells[x, y].SetRandomTile(6);
			}
		}
	}

	// Display Tile Grid
	public void DisplayTileGrid() {
		tiles = new List<MatchItem>();
		for (var x = 0; x < Data.tileWidth; x++) {
			for (var y = 0 ; y < Data.tileHeight; y++) {
				int type = (int)cells[x, y].cellType;
				Sprite sprite = sprites[(type-1)];
                GameObject instance = Instantiate(matchItemPrefab) as GameObject;
                instance.transform.parent = grid.transform;
                //instance.transform.localPosition = Vector3.zero;
                instance.GetComponent<SpriteRenderer>().sprite = sprite;
				instance.transform.localScale = Vector3.one * cellScale;
				instance.transform.localPosition = new Vector3( x * cellWidth, y * -cellHeight, 0f);
				MatchItem tile = instance.GetComponent<MatchItem>();
				tile.target = gameObject;
				tile.cell = cells[x, y];
				tile.point = new TilePoint(x, y);
                tile.spriteIndex = (type - 1);
                tiles.Add(tile);
			}
		}
	}

	// Create Tile Grid
	public void CreateTileGrid() {
		for (int x = 0; x < Data.tileWidth; x++) {
			for (var y = 0; y < Data.tileHeight; y++) {
				cells[x, y] = new Cell();
			}
		}
	}

	// Find Match-3 Tile
	private Dictionary<TilePoint, Data.TileTypes> FindMatch(Cell[,] cells) {
		Dictionary<TilePoint, Data.TileTypes> stack = new Dictionary<TilePoint, Data.TileTypes>();
		for (var x = 0; x < Data.tileWidth; x++) {
			for (var y = 0; y < Data.tileHeight; y++) {
				var thiscell = cells[x, y];
				if (thiscell.IsEmpty) continue;
				int matchCount = 0;
				int y2 = Mathf.Min(Data.tileHeight - 1, y + 2);
				int y1;
				for (y1 = y + 1; y1 <= y2 ;y1++) {
					if (cells[x, y1].IsEmpty || thiscell.cellType != cells[x, y1].cellType) break;
					matchCount++;
				}
				if (matchCount >= 2) {
					y1 = Mathf.Min(Data.tileHeight - 1, y1 - 1);
					for (var y3 = y; y3 <= y1 ;y3++) {
						if (!stack.ContainsKey( new TilePoint(x, y3) ))
							stack.Add(new TilePoint(x, y3) , cells[x, y3].cellType);
					}
				}
			}
		}
		for (var y = 0; y < Data.tileHeight; y++) {
			for (var x = 0; x < Data.tileWidth; x++) {
				var thiscell = cells[x, y];
				if (thiscell.IsEmpty) continue;                    
				int matchCount = 0;
				int x2 = Mathf.Min(Data.tileWidth - 1, x + 3);
				int x1;
				for (x1 = x + 1; x1 <= x2; x1++) {
					if (cells[x1, y].IsEmpty || thiscell.cellType != cells[x1, y].cellType) break;
					matchCount++;
				}
				if (matchCount >= 2) {
					x1 = Mathf.Min(Data.tileWidth - 1, x1 - 1);
					for (var x3 = x; x3 <= x1 ;x3++) {
						if (!stack.ContainsKey( new TilePoint(x3, y) ))
							stack.Add(new TilePoint(x3, y) , cells[x3, y].cellType);
					}
				}
			}
		}
		return stack;
	}

	// Find Match-3 Tile Hint (not sure what is the difference between hint and match 3...)
	private Dictionary<TilePoint, Data.TileTypes> FindHint() {
		Dictionary<TilePoint, Data.TileTypes> stack = new Dictionary<TilePoint, Data.TileTypes>();
		Cell[,] clone = new Cell[Data.tileWidth, Data.tileHeight];
		for (var x = 0; x < Data.tileWidth-1; x++) {
			for (var y = 0; y < Data.tileHeight; y++) {
				System.Array.Copy(cells, clone, Data.tileWidth * Data.tileHeight);
				var thiscell = clone[x, y];
				clone[x, y] = clone[x+1,y];
				clone[x+1,y] = thiscell;
				Dictionary<TilePoint, Data.TileTypes> st = new Dictionary<TilePoint, Data.TileTypes>();
				st = FindMatch(clone);
				if (st.Count>0) {
					TilePoint tp = new TilePoint(x, y);
					if (!stack.ContainsKey(tp)) 
						stack.Add(tp , clone[x, y].cellType);
					tp = new TilePoint(x+1, y);
					if (!stack.ContainsKey(tp)) 
						stack.Add(tp , clone[x+1, y].cellType);
				}
			}
		}
		for (var x = 0; x < Data.tileWidth; x++) {
			for (var y = 0; y < Data.tileHeight-1; y++) {
				System.Array.Copy(cells, clone, Data.tileWidth * Data.tileHeight);
				var thiscell = clone[x, y];
				clone[x, y] = clone[x,y+1];
				clone[x,y+1] = thiscell;
				Dictionary<TilePoint, Data.TileTypes> st = new Dictionary<TilePoint, Data.TileTypes>();
				st = FindMatch(clone);
				if (st.Count>0) {
					TilePoint tp = new TilePoint(x, y);
					if (!stack.ContainsKey(tp)) 
						stack.Add(tp , clone[x, y].cellType);
					tp = new TilePoint(x, y+1);
					if (!stack.ContainsKey(tp)) 
						stack.Add(tp , clone[x, y+1].cellType);
				}
			}
		}
		return stack;
	}

	// Do Empty Tile Move Down after matching 3
	private void DoEmptyDown() {
		for (var x = 0; x < Data.tileWidth; x++) {
			for (var y = 0; y < Data.tileHeight; y++) {
				var thiscell = cells[x, y];
				if (!thiscell.IsEmpty) continue;
				int y1;
				for (y1 = y; y1 > 0 ;y1--) {
					DoSwapTile( FindTile( new TilePoint(x, y1) ) ,  FindTile( new TilePoint(x, y1-1) ) );
				}
			}
		}
		for (var x = 0; x < Data.tileWidth; x++) {
			int y;
			for (y = Data.tileHeight-1; y >= 0; y--) {
				var thiscell = cells[x, y];
				if (thiscell.IsEmpty) break;
			}
			if (y<0) continue;
			var y1 = y;
			for (y = 0; y <= y1; y++) {
				MatchItem tile = FindTile( new TilePoint(x, y) );
				tile.transform.localPosition = new Vector3( x * cellWidth, (y-(y1+1)) * -cellHeight, 0f);
				tile.cell.SetRandomTile(6);
                int spriteIndex = (int)tile.cell.cellType - 1;
                tile.spriteIndex = spriteIndex;
                Sprite sprite= sprites[spriteIndex];
                SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.enabled = true;
			}
		}

		foreach (MatchItem tile in tiles) {
			Vector3 pos = new Vector3( tile.point.x * cellWidth, tile.point.y * -cellHeight);
			float dist = Vector3.Distance( tile.transform.localPosition , pos ) * 0.01f;
			dist = 1f;
            TweenParms parms = new TweenParms().Prop("localPosition", pos).Ease(easeType);
			HOTween.To(tile.transform, 0.5f * dist, parms );
		}
		StartCoroutine( CheckMatch3TileOnly(0.5f) );
	}

	// Find Match-3 Tile (not sure what is find tile)
	MatchItem FindTile(TilePoint point){
		foreach (MatchItem tile in tiles) {
			if (tile.point.Equals( point )) return tile;
		}
		return null;
	}

	// Find Match-3 Tile with Position
	MatchItem FindTileWithPosition(Vector2 pos){
		Vector3 pos3 = new Vector3(pos.x, pos.y, 0f) - (grid.transform.localPosition);
		MatchItem curTile = null;
		foreach(MatchItem tile in tiles) {
			if (curTile == null) {
				curTile = tile;
				continue;
			}
			float dist1 = Vector3.Distance( curTile.transform.localPosition , pos3);
			float dist2 = Vector3.Distance( tile.transform.localPosition , pos3);
			if (dist1 > dist2) {
				curTile = tile;
			}
		}
		//float dist = Vector3.Distance( curTile.transform.localPosition , pos3);
		return curTile;
	}

	// Swap Motion Animation --------------------------------------------------------------------------------- USE THIS TO MAKE AI SWIPE?
	void DoSwapMotion(Transform a, Transform b){
		Vector3 posA = a.localPosition;
		Vector3 posB = b.localPosition;
        TweenParms parms = new TweenParms().Prop("localPosition", posB).Ease(EaseType.EaseOutQuad);
		HOTween.To(a, 0.2f, parms );
        parms = new TweenParms().Prop("localPosition", posA).Ease(EaseType.EaseOutQuad);
		HOTween.To(b, 0.2f, parms );
	}

    // Swap Two Tile --------------------------------------------------------------------------------------- USE THIS TO MAKE AI SWIPE?
    void DoSwapTile(MatchItem a, MatchItem b) {
		TilePoint p1 = a.point;
		TilePoint p2 = b.point;

		Cell cell = cells[p1.x, p1.y];
		cells[p1.x, p1.y] = cells[p2.x, p2.y];
		cells[p2.x, p2.y] = cell;

		a.point = p2;
		b.point = p1;
	}

    /* --------------------------------------------------------------------------------------------DEACTIVATED ENEMY ATTACCK TO START TESTING TURN BASED ATTACK
    IEnumerator RandomMonsterAttack(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        StartCoroutine(AttackPlayer(0.7f));
        StartCoroutine(RandomMonsterAttack(Random.Range(3f, 6f)));
    }
    */

    // Attack NPC Monster
    IEnumerator AttackMonster(float delayTime) {
		pcControl.Attack();
		yield return new WaitForSeconds(delayTime);
        npcControl.Damage(0.1f);
	}

    IEnumerator AttackPlayer(float delayTime)
    {
        npcControl.Attack();
        yield return new WaitForSeconds(delayTime);
        pcControl.Damage(0.1f);
    }


	// Star Flash Effect
	void DoStarEffect(Vector3 pos, int type){

        Vector3 targetPosition = Vector3.zero;

        if (playerHumanTurn)
        {
            if(type != 5)
                targetPosition = new Vector3(200, 300, 0);
            else
                targetPosition = new Vector3(-200, 300, 0);
        }
        else
        {
            if (type != 5)
                targetPosition = new Vector3(-200, 300, 0);
            else
                targetPosition = new Vector3(200, 300, 0);
        }

        if (type<1) return;
		pos += grid.transform.localPosition + puzzle.transform.localPosition;
		GameObject starEffectObject = Instantiate(starEffectPrefabs[type-1]) as GameObject;
		Transform starEffect = starEffectObject.transform;
		starEffect.parent = panel.transform;
		Vector3[] path = new Vector3[] { pos, new Vector3(0,100,0), targetPosition };
		starEffect.localPosition = pos;
		//HOTween.To(starEffect, 0.8f, new TweenParms().Prop("localPosition", new PlugVector3Path(path, EaseType.Linear, true).OrientToPath(0.1f)));
        HOTween.To(starEffect, 0.8f, new TweenParms().Prop("localPosition", new PlugVector3Path(path, EaseType.Linear, true)).Ease(EaseType.Linear));
    }

	// Fill Empty Tile after Matching
	IEnumerator FillEmpty(float delayTime) {
		yield return new WaitForSeconds(delayTime);
		DoEmptyDown();
	}

	// Check Match-3 Tile
	void CheckMatch3(Dictionary<TilePoint, Data.TileTypes> stack){
        StartCoroutine(CheckMatch3_Coroutine(stack));
	}

    IEnumerator CheckMatch3_Coroutine(Dictionary<TilePoint, Data.TileTypes> stack)
    {

        Data.TileTypes tileType = Data.TileTypes.Empty;
        List<MatchItem> destroyList = new List<MatchItem>();
        foreach (KeyValuePair<TilePoint, Data.TileTypes> item in stack)
        {
            tileType = item.Value;
            destroyList.Add(FindTile(item.Key));
        }
        Coroutine deleteAnimation = null;
        foreach (MatchItem item in destroyList)
        {
            audioMatchSource[(int)(item.cell.cellType) - 1].Play();

            int type = (int)item.cell.cellType;


            SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
            int index = (type - 1) * 4;
            deleteAnimation = StartCoroutine(animateDelete(item, index));

            GameObject instance = Instantiate(explosionPrefab) as GameObject;
            instance.transform.parent = effectArea;
            instance.transform.localPosition = new Vector3(item.point.x * cellWidth, item.point.y * -cellHeight, -5f);

            DoStarEffect(instance.transform.localPosition, type);
        }
        yield return deleteAnimation;

        //Calculates the effect Values
        if (playerHumanTurn)
            CalculateElementEffect(tileType,npcControl.baseElement);
        else
            CalculateElementEffect(tileType,pcControl.baseElement);

        CompleteAttack(playerHumanTurn);
    }


    void CompleteAttack(bool isPlayerHuman)
    {
        if (!nullifyAttack)
        {
            if (isPlayerHuman)
            {
                StartCoroutine(AttackMonster(0.7f));
                StartCoroutine(FillEmpty(0.5f));
            }
            else
            {
                StartCoroutine(AttackPlayer(0.7f));
                StartCoroutine(FillEmpty(0.5f));
            }
        }
        else
        {
            StartCoroutine(FillEmpty(0.5f));
        }
    }

    //Make animation fade out after matching tiles and before creating new tiles to drop down
    IEnumerator animateDelete(MatchItem item, int index)
    {
        SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
        for (int i = 0; i < 4; i++)
        {
            sr.sprite = deleteSprites[index + i];
            yield return new WaitForSeconds(0.1f);
        }

        GameObject instance = Instantiate(explosionPrefab) as GameObject;
        instance.transform.parent = effectArea;
        instance.transform.localPosition = new Vector3(item.point.x * cellWidth, item.point.y * -cellHeight, -5f);
        item.cell.cellType = Data.TileTypes.Empty;
        sr.enabled = false;
    }

	// Find Hint Debug (it tells possible combinations in the board)
	void DebugFindHint(){
		Dictionary<TilePoint, Data.TileTypes> st = FindHint();
		string str = "";
		foreach (KeyValuePair<TilePoint, Data.TileTypes> item in st){
			str += item.Key + ", ";
		}
		Debug.Log("FindHint: " + str);

        Dictionary<TilePoint, Data.TileTypes> matches = FindMatch(cells);
        string newString = "";
        foreach (KeyValuePair<TilePoint, Data.TileTypes> item in matches)
        {
            newString += item.Key + ", ";
        }
        Debug.Log("FindMatch: " + newString);
    }


    //Making the AI recognize possible matches after the player turn
    IEnumerator ReadyEnemyTurn()
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("Enemy turn start");
        Dictionary<TilePoint, Data.TileTypes> st = FindHint();

        List<TilePoint> tileMatchList = new List<TilePoint>();

        foreach (KeyValuePair<TilePoint, Data.TileTypes> item in st)
        {
            tileMatchList.Add(item.Key);
        }

        // Divide size of match list in 2 to get actual number of matches
        int firstMatchIndex = tileMatchList.Count / 2;

        // Choose one of those matches randomly
        int randomMatchIndex = Random.Range(0, firstMatchIndex);

        // Get the first tile coordinate of one of those random matches
        TilePoint tile1 = tileMatchList[randomMatchIndex * 2];

         // Get the second tile coordinate of one of those random matches
        TilePoint tile2 = tileMatchList[randomMatchIndex * 2 + 1];

        MatchItem item1 = FindTile(tile1);
        MatchItem item2 = FindTile(tile2);

        isDoing = true;
        DoSwapTile(item1, item2);
        yield return StartCoroutine(CheckMatch3Tile(0.5f, item1, item2));

        Debug.Log("AI turn end");
    }

	// Ready Game Turn
	void ReadyGameTurn(){

        if (bonusTurns == 0)
        {
            playerHumanTurn = !playerHumanTurn;

            //Disable freeze icon
            pcControl.EnableFreezeStatus(false);
            npcControl.EnableFreezeStatus(false);
        }
        else
        {
            bonusTurns--;
        }

        if (isPlayerHumanTurn())
        {
            isDoing = false;
            canDoInput = true;
            pcControl.EnableActiveBackground(true);
            npcControl.EnableActiveBackground(false);
            DebugFindHint();
        }
        else
        {
            pcControl.EnableActiveBackground(false);
            npcControl.EnableActiveBackground(true);
            DoAITurn();
        }
	}

    bool isPlayerHumanTurn()
    {
        return playerHumanTurn;
    }

    void DoAITurn()
    {
        StartCoroutine(ReadyEnemyTurn());
    }


    void DoTileEffect(Data.TileTypes tileType)
    {
        /* switch (tileType) ------------------------------------------------------- Create different effects to the different pieces (eg. ice freezes enemy, heart gives life, etc.)
       
        //player life = 350, Enemy life Level 1 = 270

       //3 pieces same type = 18 pts damage
       //4 pieces same type = 28 pts damage
       //more than 4 pieces same type = 42 damage 
       //3 strong pieces (considering element) = 30 pts damage
       //4 strong pieces (considering element) = 45 pts damage
       //more than 4 strong pieces (considering element) = 60 pts damage
       
        //3 pieces life = 20 pts life back
        //4 pieces life = 32 pts life back
        ///more than 4 pieces life = 45 pts life back

       //3 pieces white = enemy does not play that turn 
       //4 white = enemy does not play for 2 turns
       //more than 4 white = enemy does not play for 3 turns (?)

        {
            
        case Data.TileTypes.White:

                // What des white tiles do?


                break;

        case Data.TileTypes.Red:
                if (isPlayerTurn)
                {
                    npcControl.Damage(10, Element.FIRE);
                }
                else
                {
                    pcControl.Damage(10, Element.FIRE);
                }

                break;

        case Data.TileTypes.Blue:

                // What do blue tiles do?

                break;

        
        case Data.TileTypes.Magnet:

               // What do magnet tiles do?

                break;

        case Data.TileTypes.Green:

                // Put code to heal the player here

                break;

        case Data.TileTypes.Yellow:

                // normal attack: 10 points

                break;
            
        } */
    }

    // Check Only Match-3 Tile
    IEnumerator CheckMatch3TileOnly(float delayTime) {
		yield return new WaitForSeconds(delayTime);
		Dictionary<TilePoint, Data.TileTypes> stack = FindMatch(cells);
		if (stack.Count>0) {
            lastStackCount = stack.Count;
            nullifyAttack = false;
            CheckMatch3(stack);
		} else {
            lastStackCount = 0;
            nullifyAttack = false;
            ReadyGameTurn();
		}
	}

	// Check Match-3 Tile
	IEnumerator CheckMatch3Tile(float delayTime, MatchItem a, MatchItem b) {
		yield return new WaitForSeconds(delayTime);
		Dictionary<TilePoint, Data.TileTypes> stack = FindMatch(cells);
        canDoInput = false;

        if (playerHumanTurn)
        {
            if (stack.Count > 0)
            {
                lastStackCount = stack.Count;
                CheckMatch3(stack);
            }
            else
            {
                DoSwapTile(a, b);
                DoSwapMotion(a.transform, b.transform);
                isDoing = false;
                canDoInput = true;
            }
        }

        //AI turn
        else
        {
            if (stack.Count > 0)
            {
                lastStackCount = stack.Count;
                DoSwapMotion(a.transform, b.transform);
                CheckMatch3(stack);
            }
            else
            {
                DoSwapTile(a, b);
                DoAITurn();
            }
        }
	
	}
    
	// Click Event - highlighting the piece the playerr is holding to swipe
	void OnClickAction(MatchItem tile){
        //StartCoroutine(animateDelete(tile.GetComponent<SpriteRenderer>(), (tile.spriteIndex) * 4));
        
		if (isDoing) return;

		if (tile==null) return;
        if (curTile == null)
        {
            curTile = tile;
            curTile.gameObject.GetComponent<SpriteRenderer>().sprite = spritesSelected[tile.spriteIndex];
            choice.transform.localPosition = curTile.transform.localPosition;
            choice.enabled = true;
        }
        
	}

    //Swiping the pieces by drag and drop
    void OnClickUpAction(MatchItem tile)
    {
        if (curTile != null)
        {
            if (Mathf.Abs(curTile.point.x - tile.point.x) + Mathf.Abs(curTile.point.y - tile.point.y) != 1)
            {
                curTile.gameObject.GetComponent<SpriteRenderer>().sprite = sprites[curTile.spriteIndex];
                curTile = null;
                choice.enabled = false;
                return;
            }
            isDoing = true;
            DoSwapTile(curTile, tile);
            DoSwapMotion(curTile.transform, tile.transform);
            StartCoroutine(CheckMatch3Tile(0.5f, curTile, tile));
            curTile.gameObject.GetComponent<SpriteRenderer>().sprite = sprites[curTile.spriteIndex];
            curTile = null;
            choice.enabled = false;
        }
    }
        void GotoMenu()
    {
        SceneManager.LoadScene("BTMap");
    }

    private float CalculateHealValue()
    {
        if (lastStackCount == 3)
        {
            healValue = 0.2f;
        }
        else if (lastStackCount == 4)
        {
            healValue = 0.32f;
        }
        else if (lastStackCount > 4)
        {
            healValue = 0.45f;
        }

        return healValue;
    }
    private int CalculateBonusTurns()
    {
        if (lastStackCount == 3)
        {
            bonusTurns++;
        }
        else if (lastStackCount == 4)
        {
            bonusTurns = bonusTurns+2;
        }
        else if (lastStackCount > 4)
        {
            bonusTurns= bonusTurns+3;
        }

        return bonusTurns;
    }

    private void CalculateElementEffect(Data.TileTypes attackElementType, Data.TileTypes targetElementType)
    {
        if (attackElementType == Data.TileTypes.Blue)
        {
            WaterElementEffect(targetElementType);
        }
        else if (attackElementType == Data.TileTypes.Green)
        {
            //Heal the player who swiped
            if (playerHumanTurn)
            {
                healValue = CalculateHealValue();
                pcControl.SetHealthUp(healValue);
                Debug.Log("Player healed: " + healValue);
                nullifyAttack = true;
            }
            else
            {
                healValue = CalculateHealValue();
                npcControl.SetHealthUp(healValue);
                Debug.Log("Enemy healed: " + healValue);
                nullifyAttack = true;
            }
        }
        else if (attackElementType == Data.TileTypes.Magnet)
        {
            MagnetElementEffect(targetElementType);
        }
        else if (attackElementType == Data.TileTypes.Red)
        {
            FireElementEffect(targetElementType);
        }
        else if (attackElementType == Data.TileTypes.White)
        {
            bonusTurns = CalculateBonusTurns();
            Debug.Log("Bonus turns: " + bonusTurns);

            if (playerHumanTurn)
            {
                npcControl.EnableFreezeStatus(true);
            }
            else
            {
                pcControl.EnableFreezeStatus(true);
            }

        }
        else if (attackElementType == Data.TileTypes.Yellow)
        {
            LightningElementEffect(targetElementType);
        }


    }

    //Functions handle the effects of each element with the target element
    private void WaterElementEffect(Data.TileTypes targetElementType)
    {
        if (targetElementType == Data.TileTypes.Blue)
        {

        }
        else if (targetElementType == Data.TileTypes.Green)
        {

        }
        else if (targetElementType == Data.TileTypes.Magnet)
        {

        }
        else if (targetElementType == Data.TileTypes.Red)
        {

        }
        else if (targetElementType == Data.TileTypes.White)
        {

        }
        else if (targetElementType == Data.TileTypes.Yellow)
        {

        }
    }
    private void LeafElementEffect(Data.TileTypes targetElementType)
    {
        if (targetElementType == Data.TileTypes.Blue)
        {

        }
        else if (targetElementType == Data.TileTypes.Green)
        {

        }
        else if (targetElementType == Data.TileTypes.Magnet)
        {

        }
        else if (targetElementType == Data.TileTypes.Red)
        {

        }
        else if (targetElementType == Data.TileTypes.White)
        {

        }
        else if (targetElementType == Data.TileTypes.Yellow)
        {

        }
    }
    private void MagnetElementEffect(Data.TileTypes targetElementType)
    {
        if (targetElementType == Data.TileTypes.Blue)
        {

        }
        else if (targetElementType == Data.TileTypes.Green)
        {

        }
        else if (targetElementType == Data.TileTypes.Magnet)
        {

        }
        else if (targetElementType == Data.TileTypes.Red)
        {

        }
        else if (targetElementType == Data.TileTypes.White)
        {

        }
        else if (targetElementType == Data.TileTypes.Yellow)
        {

        }
    }
    private void FireElementEffect(Data.TileTypes targetElementType)
    {
        if (targetElementType == Data.TileTypes.Blue)
        {

        }
        else if (targetElementType == Data.TileTypes.Green)
        {

        }
        else if (targetElementType == Data.TileTypes.Magnet)
        {

        }
        else if (targetElementType == Data.TileTypes.Red)
        {

        }
        else if (targetElementType == Data.TileTypes.White)
        {

        }
        else if (targetElementType == Data.TileTypes.Yellow)
        {

        }
    }
    private void IceElementEffect(Data.TileTypes targetElementType)
    {
        if (targetElementType == Data.TileTypes.Blue)
        {

        }
        else if (targetElementType == Data.TileTypes.Green)
        {

        }
        else if (targetElementType == Data.TileTypes.Magnet)
        {

        }
        else if (targetElementType == Data.TileTypes.Red)
        {

        }
        else if (targetElementType == Data.TileTypes.White)
        {

        }
        else if (targetElementType == Data.TileTypes.Yellow)
        {

        }
    }
    private void LightningElementEffect(Data.TileTypes targetElementType)
    {
        if (targetElementType == Data.TileTypes.Blue)
        {

        }
        else if (targetElementType == Data.TileTypes.Green)
        {

        }
        else if (targetElementType == Data.TileTypes.Magnet)
        {

        }
        else if (targetElementType == Data.TileTypes.Red)
        {

        }
        else if (targetElementType == Data.TileTypes.White)
        {

        }
        else if (targetElementType == Data.TileTypes.Yellow)
        {

        }
    }

    // Start Game - setting the level
    void Start () {

        //Initialize values for player to start
        isDoing = false;
        canDoInput = true;
        playerHumanTurn = true;
        pcControl.EnableActiveBackground(true);

        //Set the elements for the player and the enemy
        int pcControlElement = Random.Range(0, 6);
        pcControl.SetElement(pcControlElement);
        pcControl.elementSprite.sprite = sprites[pcControlElement];
        int npcControlElement = Random.Range(0, 6);
        npcControl.elementSprite.sprite = sprites[npcControlElement];
        npcControl.SetElement(npcControlElement);

		SetupAudioSource();
		choice.enabled = false;
		CreateTileGrid();
		while (true){
			InitTileGrid();
			Dictionary<TilePoint, Data.TileTypes> stack = FindMatch(cells);
			if (stack.Count<1) break;
		}
		DisplayTileGrid();
        //StartCoroutine(RandomMonsterAttack(Random.Range(3f, 6f))); -------------------------------------------------------------------------------------------------- not sure why? 
	}

    private void Update()
    {
        if ( canDoInput ) {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, LayerMask.GetMask("UI2D")))
                {
                    if (hit.transform.gameObject.layer == 8)
                    {
                        if (hit.transform.gameObject.GetComponent<MatchItem>())
                        {
                            OnClickAction(hit.transform.gameObject.GetComponent<MatchItem>());
                        }
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, LayerMask.GetMask("UI2D")))
            {
                    if (hit.transform.gameObject.layer == 8)
                    {
                        if (hit.transform.gameObject.GetComponent<MatchItem>())
                        {
                            OnClickUpAction(hit.transform.gameObject.GetComponent<MatchItem>());
                        }
                    }
                }
            }
        }
    }
}

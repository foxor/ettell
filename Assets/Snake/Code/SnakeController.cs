using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SnakeController : MonoBehaviour {
	
	private enum Direction {
		up,
		down,
		left,
		right
	}
	
	private struct Position {
		public int x;
		public int y;
		
		public Position(int x, int y) {
			this.x = x;
			this.y = y;
		}
	}
	
	private const int MAX_TAIL_LENGTH = 100;
	private const float FASTEST_TPS = 0.02f;
	
	public float timePerSquare;
	
	private Direction direction;
	private Direction lastDirection;
	private Position goingFrom;
	private Position goingTo;
	
	private float curTime;
	
	private Queue<Position> newlyFilledPositions;
	private Queue<Position> filledPositions;
	private Queue<Position> oldPositions;
	
	private int tailLength;
	
	private bool interactive;
	private Position[] plan;
	private int planIndex;
	private int nodesIntoEnding;
	private bool gameOver;
	
	public void Start() {
		direction = Direction.up;
		goingTo = new Position(SnakeGame.Width / 2, SnakeGame.Height / 2);
		goingFrom = new Position(0, 0);
		curTime = timePerSquare;
		newlyFilledPositions = new Queue<Position>();
		filledPositions = new Queue<Position>();
		interactive = true;
		tailLength = 2;
	}
	
	private void onChompSquare() {
		if (!interactive) {
			return;
		}
		
		newlyFilledPositions.Enqueue(goingFrom);
		
		Color eating = SnakeGame.Singleton[goingTo.x, goingTo.y];
		if (eating == SnakeGame.EMPTY_COLOR) {
			return;
		}
		
		if (eating == SnakeGame.Singleton.BadStock) {
			if (--tailLength <= 0) {
				SnakeGame.ettellLose();
			}
		}
		else if (eating == SnakeGame.Singleton.GoodStock) {
			tailLength++;
		}
		else if (eating == SnakeGame.Singleton.BorderColor) {
			interactive = false;
			if (tailLength < SnakeGame.WinThreshold) {
				SnakeGame.ettellLose();
				gameOver = true;
				return;
			}
			plan = filledPositions.Reverse().Concat(filledPositions).ToArray();
			planIndex = 0;
			nodesIntoEnding = 0;
		} 
		else if (eating == SnakeGame.Singleton.EttellStart) {
			interactive = false;
			IEnumerable<Position> filledRoute = filledPositions.Reverse().TakeWhile(p => p.x != goingTo.x || p.y != goingTo.y).Reverse();
			plan = new Position[filledRoute.Count() + newlyFilledPositions.Count + 1];
			if (plan.Length < SnakeGame.WinThreshold) {
				SnakeGame.ettellLose();
				gameOver = true;
				return;
			}

			plan[0] = goingTo;
			System.Array.Copy(filledRoute.ToArray(), 0, plan, 1, filledRoute.Count());
			System.Array.Copy(newlyFilledPositions.ToArray(), 0, plan, 1 + filledRoute.Count(), newlyFilledPositions.Count());
			planIndex = 0;
			nodesIntoEnding = 0;
		}
	}
	
	private void setPos() {
		while (curTime >= timePerSquare && !gameOver) {
			curTime -= timePerSquare;
			goingFrom.x = goingTo.x;
			goingFrom.y = goingTo.y;
			lastDirection = direction;
			if (interactive) {
				goingTo.x = goingFrom.x + (direction == Direction.right ? 1 : (direction == Direction.left ? -1 : 0));
				goingTo.y = goingFrom.y + (direction == Direction.up ? 1 : (direction == Direction.down ? -1 : 0));
			}
			else {
				planIndex = (planIndex + 1) % plan.Length;
				nodesIntoEnding++;
				int endingNo = (nodesIntoEnding / SnakeGame.NodesPerLerp);
				float endingRatio = ((float)(nodesIntoEnding % SnakeGame.NodesPerLerp)) / ((float)SnakeGame.NodesPerLerp);
				if (endingNo + 1 >= SnakeGame.Singleton.EttellLerp.Length) {
					SnakeGame.cycleEnd();
					return;
				}
				Color newEttellColor = Color.Lerp(SnakeGame.Singleton.EttellLerp[endingNo], SnakeGame.Singleton.EttellLerp[endingNo + 1], endingRatio);
				foreach (Position p in filledPositions) {
					SnakeGame.Singleton[p.x, p.y] = newEttellColor;
				}
				goingTo = plan[planIndex];
				
				timePerSquare = Mathf.Max(FASTEST_TPS, timePerSquare / 1.07f);
			}
			
			onChompSquare();
		}
		
		if (Camera.main != null && !gameOver) {
			Vector3 cameraPos = Camera.main.transform.position;
			Vector3 myPos = transform.position;
		
			cameraPos.x = myPos.x = Mathf.Lerp((float)goingFrom.x, (float)goingTo.x, curTime / timePerSquare);
			cameraPos.y = myPos.y = Mathf.Lerp((float)goingFrom.y, (float)goingTo.y, curTime / timePerSquare);
		
			Camera.main.transform.position = cameraPos;
			transform.position = myPos;
		}
	}
	
	private void manageTail() {
		if (gameOver) {
			return;
		}
		while (newlyFilledPositions.Any()) {
			Position p = newlyFilledPositions.Dequeue();
			SnakeGame.Singleton[p.x, p.y] = SnakeGame.Singleton.EttellStart;
			filledPositions.Enqueue(p);
		}
		if (interactive) {
			while (filledPositions.Count > tailLength) {
				Position p = filledPositions.Dequeue();
				SnakeGame.Singleton[p.x, p.y] = SnakeGame.EMPTY_COLOR;
			}
		}
	}
	
	public void Update() {
		if ((Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) && lastDirection != Direction.right) {
			direction = Direction.left;
		}
		if ((Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) && lastDirection != Direction.left) {
			direction = Direction.right;
		}
		if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) && lastDirection != Direction.down) {
			direction = Direction.up;
		}
		if ((Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) && lastDirection != Direction.up) {
			direction = Direction.down;
		}
		
		curTime += Time.deltaTime;
		
		setPos();
		manageTail();
	}
}
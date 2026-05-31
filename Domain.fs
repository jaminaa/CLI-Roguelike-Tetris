namespace RoguelikeTetris

module Domain =

  
    type Pos = { X: int; Y: int }

  
    type ShapeType = I | O | T | S | Z | J | L

  
    type Tetromino = {
        Shape: ShapeType
        Position: Pos
        Rotation: int  
    }

  
    type StageModifier = 
        | FogOfWar  
        | GarbageRise  

    type PowerUp = 
        | SlowerGravity
        | DoublePoints
        | ExtraHoldSlots
        | QuickClear  

  
  
  
    type Board = ShapeType option list list

  
    type GameStatus = 
        | MainMenu
        | Playing
        | PowerUpSelection  
        | BetweenStages  
        | GameOver
        | EndlessMode  

  
  
    type GameState = {
        Board: Board
        CurrentPiece: Tetromino
        NextPiece: Tetromino
        HoldSlot1: Tetromino option
        HoldSlot2: Tetromino option
        HasExtraSlot: bool
        Score: int
        Level: int
        LinesClearedInStage: int
        LinesRequiredForNextStage: int
        FallingSpeed: float  
        ActivePowerUps: PowerUp list  
        ActiveModifiers: StageModifier list
        Status: GameStatus
        RemainingHolds: int 
        PiecesPlacedInStage: int
        QuickClearCharges: int 

    }

  
    type ScoreEntry = {
        PlayerName: string
        FinalScore: int
        StageReached: int
        Date: System.DateTime
    }

namespace RoguelikeTetris

open System
open Domain
open Tetrominoes
open Board
open Progression
open UI

module GameLoop =

  
    type Action =UseQuickClear | MoveLeft | MoveRight | RotateCW | RotateCCW | SoftDrop | HardDrop | HoldPrimary | HoldSecondary | Tick | NoOp

  
    let getAction() =
        if Console.KeyAvailable then
            match Console.ReadKey(true).Key with
            | ConsoleKey.Q -> UseQuickClear
            | ConsoleKey.LeftArrow  -> MoveLeft
            | ConsoleKey.RightArrow -> MoveRight
            | ConsoleKey.Z          -> RotateCCW
            | ConsoleKey.X          -> RotateCW
            | ConsoleKey.DownArrow  -> SoftDrop
            | ConsoleKey.Spacebar   -> HardDrop
            | ConsoleKey.H          -> HoldSecondary
            | ConsoleKey.C          -> HoldPrimary
            | _ -> NoOp
        else NoOp

  
    let rec update (state: GameState) (action: Action) : GameState =
        match action with
        | MoveLeft -> 
            let moved = move -1 0 state.CurrentPiece
            if isValid state.Board moved then { state with CurrentPiece = moved } else state
        
        | MoveRight -> 
            let moved = move 1 0 state.CurrentPiece
            if isValid state.Board moved then { state with CurrentPiece = moved } else state

        | RotateCW | RotateCCW ->
            let rotated = rotate (action = RotateCW) state.CurrentPiece
            if isValid state.Board rotated then { state with CurrentPiece = rotated } else state

        | Tick | SoftDrop ->
            let dropped = move 0 1 state.CurrentPiece
            if isValid state.Board dropped then
                { state with CurrentPiece = dropped }
            else
  
                let newBoard = placePiece state.Board state.CurrentPiece
                let (clearedBoard, lines) = clearLines newBoard
                
                let newPiecesCount = state.PiecesPlacedInStage + 1
                let isEndless = (state.Status = EndlessMode)
                
  
                let currentInterval = if isEndless then Progression.getEndlessGarbageInterval newPiecesCount else 10
                let isGarbageActive = state.ActiveModifiers |> List.contains GarbageRise || isEndless
                
  
                let boardWithGarbage = 
                    if isGarbageActive && newPiecesCount % currentInterval = 0 then
                        Board.addGarbage clearedBoard
                    else clearedBoard

                if not (isValid boardWithGarbage state.NextPiece) then
                    { state with Status = GameOver }
                else
                    let points = Progression.getModifiedScore lines state.ActivePowerUps
                    let newSpeed = if isEndless then Progression.getEndlessSpeed newPiecesCount state.ActivePowerUps else state.FallingSpeed

  
                    let newState = { state with 
                                        Board = boardWithGarbage
                                        Score = state.Score + points
                                        PiecesPlacedInStage = newPiecesCount
  
                                        LinesClearedInStage = state.LinesClearedInStage + lines 
  
                                        FallingSpeed = newSpeed
                                        CurrentPiece = state.NextPiece
                                        NextPiece = spawnRandom()
                                        RemainingHolds = Progression.getMaxHolds state.ActivePowerUps }
                    
  
  
                    if state.Status = Playing && Progression.isStageComplete newState then 
                        { newState with Status = PowerUpSelection }
                    else 
                        newState

        | HardDrop -> 
            let rec drop (s: GameState) =
                let moved = move 0 1 s.CurrentPiece
                if isValid s.Board moved then drop { s with CurrentPiece = moved }
                else s
            let landedState = drop state
            update landedState Tick  

        | HoldPrimary | HoldSecondary ->
            let isSecondary = (action = HoldSecondary)
  
            if isSecondary && not state.HasExtraSlot then state
            elif state.RemainingHolds <= 0 then state
            else
                let spawnPos = { X = 3; Y = 0 }
                let current = { state.CurrentPiece with Position = spawnPos; Rotation = 0 }
                
  
                let targetSlot = if isSecondary then state.HoldSlot2 else state.HoldSlot1
                
                match targetSlot with
                | Some held ->
                    let newState = 
                        if isSecondary then { state with HoldSlot2 = Some current }
                        else { state with HoldSlot1 = Some current }
                    { newState with 
                        CurrentPiece = { held with Position = spawnPos; Rotation = 0 }
                        RemainingHolds = state.RemainingHolds - 1 }
                | None ->
                    let newState = 
                        if isSecondary then { state with HoldSlot2 = Some current }
                        else { state with HoldSlot1 = Some current }
                    { newState with 
                        CurrentPiece = { state.NextPiece with Position = spawnPos; Rotation = 0 }
                        NextPiece = spawnRandom()
                        RemainingHolds = state.RemainingHolds - 1 }
        | UseQuickClear ->
            if state.QuickClearCharges > 0 then
  
                let newBoard = Board.useQuickClear state.Board 1 
                { state with 
                    Board = newBoard
                    QuickClearCharges = state.QuickClearCharges - 1 }
            else state

        | _ -> state

  
    let rec runGame (state: GameState) (lastTick: DateTime) = async {
        drawGame state

        let action = getAction()
        let nextState = update state action 

        let now = DateTime.Now
        let timeSinceTick = (now - lastTick).TotalMilliseconds

  
        if nextState.Status = Playing || nextState.Status = EndlessMode then
            if timeSinceTick > state.FallingSpeed then
                let tickedState = update nextState Tick
                do! Async.Sleep 33
                return! runGame tickedState now
            else
                do! Async.Sleep 33
                return! runGame nextState lastTick
        
        elif nextState.Status = PowerUpSelection then
            let options = Progression.getRandomPowerUpOptions nextState.ActivePowerUps
            UI.drawPowerUpSelection options
            
            let rec getSelection() =
                match Console.ReadKey(true).Key with
                | ConsoleKey.D1 -> 0
                | ConsoleKey.D2 -> 1
                | ConsoleKey.D3 -> 2
                | _ -> getSelection()

            let picked = options.[getSelection()]
            
  
            let finalPowerUps = 
                if nextState.ActivePowerUps.Length < 3 then
  
                    picked :: nextState.ActivePowerUps 
                else
  
                    UI.drawReplacementMenu nextState.ActivePowerUps picked
                    let rec getReplaceChoice() =
                        match Console.ReadKey(true).Key with
                        | ConsoleKey.D1 -> Progression.replacePowerUp nextState.ActivePowerUps 0 picked
                        | ConsoleKey.D2 -> Progression.replacePowerUp nextState.ActivePowerUps 1 picked
                        | ConsoleKey.D3 -> Progression.replacePowerUp nextState.ActivePowerUps 2 picked
                        | ConsoleKey.D4 -> nextState.ActivePowerUps  
                        | _ -> getReplaceChoice()
                    getReplaceChoice()

  
            let updatedState = { nextState with ActivePowerUps = finalPowerUps }
            
            if updatedState.Status = EndlessMode then
  
                let progressedState = Progression.startNextStage updatedState
                Console.Clear()
                return! runGame progressedState DateTime.Now
            else
  
                let betweenState = { updatedState with Status = BetweenStages }
                Console.Clear()
                return! runGame betweenState DateTime.Now

        elif nextState.Status = BetweenStages then
            drawBetweenStagesMenu nextState

  
            let rec getChoice() =
                match Console.ReadKey(true).Key with
                | ConsoleKey.S -> 1  
                | ConsoleKey.N -> 2  
                | ConsoleKey.E when nextState.Level >= 5 -> 3  
                | _ -> getChoice()

            let userChoice = getChoice()
            
  
            match userChoice with
            | 1 ->  
                SaveSystem.saveGame nextState
                Console.Clear()
                return ()  

            | 2 ->  
                let progressedState = startNextStage nextState
                Console.Clear()
                return! runGame progressedState DateTime.Now

            | 3 ->  
  
                let endlessState = { nextState with 
                                        Status = EndlessMode 
                                        ActiveModifiers = [FogOfWar; GarbageRise] }
                let progressedState = startNextStage endlessState
                Console.Clear()
                return! runGame progressedState DateTime.Now

            | _ -> return! runGame nextState DateTime.Now


        elif nextState.Status = GameOver then
            Console.Clear()
            printfn "GAME OVER! Final Score: %d" nextState.Score
            printf "Enter your name: "
            let name = Console.ReadLine()
            SaveSystem.saveScore { PlayerName = name; FinalScore = nextState.Score; StageReached = nextState.Level; Date = DateTime.Now }
            printfn "Press any key to return to menu..."
            Console.ReadKey() |> ignore
        
        else
            return ()
    }
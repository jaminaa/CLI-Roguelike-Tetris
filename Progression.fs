namespace RoguelikeTetris

module Progression =
    open Domain
    
    let rnd = System.Random()

  
    let getRandomPowerUpOptions (activePowerUps: PowerUp list) =
        let allPowerUps = [SlowerGravity; DoublePoints; ExtraHoldSlots; QuickClear]
        
  
        let available = 
            if activePowerUps |> List.contains ExtraHoldSlots then
                allPowerUps |> List.filter (fun p -> p <> ExtraHoldSlots)
            else allPowerUps

        available 
        |> List.sortBy (fun _ -> rnd.Next()) 
        |> List.take (min 3 available.Length)

  
  
    let getPowerUpDescription (p: PowerUp) =
        match p with
        | SlowerGravity  -> "Falling speed is 30% slower per stack."
        | DoublePoints   -> "Points are doubled per stack."
        | ExtraHoldSlots -> "Unlock the 'H' key for a 2nd hold slot."
        | QuickClear     -> "Press [Q] to clear 2 rows.(1 charge per stack)"

    let getModifiedSpeed stage (activePowerUps: PowerUp list) =
        let baseSpeed = 1000.0 * (0.8 ** float (stage - 1))
        let count = activePowerUps |> List.filter (fun p -> p = SlowerGravity) |> List.length
        baseSpeed * (1.3 ** float count)

    let getModifiedScore lines (activePowerUps: PowerUp list) =
        let basePoints = lines * 100
        let count = activePowerUps |> List.filter (fun p -> p = DoublePoints) |> List.length
        basePoints * (pown 2 count)

    let getMaxHolds activePowerUps =
        let count = activePowerUps |> List.filter (fun p -> p = ExtraHoldSlots) |> List.length
        1 + count
    
    let getMaxQuickClearCharges activePowerUps =
        activePowerUps |> List.filter (fun p -> p = QuickClear) |> List.length


    let getStageRequirements stage =
        let linesNeeded = 20 + (stage * 10)
        let speed = 1000.0 * (0.8 ** float (stage - 1))
        (linesNeeded, speed)

    let isStageComplete (state: GameState) =
        state.LinesClearedInStage >= state.LinesRequiredForNextStage

    let getRandomModifier () =
        let modifiers = [FogOfWar; GarbageRise]
        modifiers.[rnd.Next(modifiers.Length)]

  
    let startNextStage (state: GameState) =
        let nextLevel=state.Level + 1
        let (lines, speed) = getStageRequirements nextLevel
        let newModifier = getRandomModifier()
        let qcCharges = getMaxQuickClearCharges state.ActivePowerUps
        let updatedModifiers = 
            if state.Status = EndlessMode then
                newModifier :: state.ActiveModifiers  
            else
                [newModifier]  

        let hasExtra = state.ActivePowerUps |> List.contains ExtraHoldSlots
        
  
        { state with 
            Status = if state.Status = EndlessMode then EndlessMode else Playing
            Level = nextLevel
            Board = Board.emptyBoard
            LinesClearedInStage = 0
            LinesRequiredForNextStage = lines
            FallingSpeed = getModifiedSpeed nextLevel state.ActivePowerUps
            RemainingHolds = getMaxHolds state.ActivePowerUps
            PiecesPlacedInStage = if state.Status = EndlessMode then state.PiecesPlacedInStage else 0
            ActiveModifiers = [getRandomModifier()] 
            HasExtraSlot = hasExtra 
            QuickClearCharges = qcCharges
            }

  
    let replacePowerUp (active: PowerUp list) (index: int) (newPower: PowerUp) =
        active |> List.mapi (fun i p -> if i = index then newPower else p)

    let getEndlessSpeed pieces activePowerUps =
        let baseSpeed = 1000.0 * (0.98 ** float (pieces / 5))
        let count = activePowerUps |> List.filter (fun p -> p = SlowerGravity) |> List.length
        let slowedSpeed = baseSpeed * (1.3 ** float count)
        slowedSpeed
    
    let getEndlessGarbageInterval pieces =
        let interval = 10 - (pieces / 100)
        max 5 interval  



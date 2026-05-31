module RoguelikeTetris.UI

open System
open System.Text
open Domain
open Tetrominoes

  
let colorCode = function
    | I -> "\u001b[38;2;70;180;180m"  
    | O -> "\u001b[38;2;190;170;60m"  
    | T -> "\u001b[38;2;140;90;180m"  
    | S -> "\u001b[38;2;80;160;80m"  
    | Z -> "\u001b[38;2;180;70;70m"  
    | J -> "\u001b[38;2;70;110;180m"  
    | L -> "\u001b[38;2;190;130;60m"  

let reset = "\u001b[0m"
let frameColor = "\u001b[38;2;60;60;70m"  
let statsColor = "\u001b[38;2;140;140;150m"  

let drawGame (state: GameState) =
    let sb = StringBuilder()
    Console.SetCursorPosition(0, 0)

  
    let fallingBlocks = getGlobalBlocks state.CurrentPiece
    let ghostPiece = Board.getGhostPiece state.Board state.CurrentPiece
    let ghostBlocks = getGlobalBlocks ghostPiece

  
    for y in 0 .. 25 do
        let line = StringBuilder()
        
  
        if y = 0 then 
            line.Append("    " + frameColor + "┏━━━━━━━━━━━━━━━━━━━━┓" + reset) |> ignore
        elif y >= 1 && y <= 20 then 
            line.Append("    " + frameColor + "┃" + reset) |> ignore
            let boardY = y - 1
            let boardRow = state.Board.[boardY]
            
            for x in 0 .. 9 do
                let isFalling = fallingBlocks |> List.exists (fun p -> p.X = x && p.Y = boardY)
                let isGhost = ghostBlocks |> List.exists (fun p -> p.X = x && p.Y = boardY)
                
                if isFalling then 
                    line.Append(colorCode state.CurrentPiece.Shape + "██" + reset) |> ignore
                elif isGhost then 
                    line.Append("\u001b[38;2;45;45;50m░░" + reset) |> ignore
                else
                    match boardRow.[x] with
                    | Some shape -> line.Append(colorCode shape + "██" + reset) |> ignore
                    | None -> line.Append("  ") |> ignore
            line.Append(frameColor + "┃" + reset) |> ignore
        elif y = 21 then
            line.Append("    " + frameColor + "┗━━━━━━━━━━━━━━━━━━━━┛" + reset) |> ignore
        else
            line.Append("                          ") |> ignore

  
        line.Append(statsColor) |> ignore
        match y with
        | 1 -> line.Append(sprintf "  SCORE: %-10d" state.Score) |> ignore
        | 2 -> 
            if state.Status = EndlessMode then 
                line.Append(sprintf "  MODE: ENDLESS (PCS: %d)" state.PiecesPlacedInStage) |> ignore
            else 
                line.Append(sprintf "  STAGE: %-10d" state.Level) |> ignore
        | 3 -> 
            if state.Status = EndlessMode then
                let interval = Progression.getEndlessGarbageInterval state.PiecesPlacedInStage
                line.Append(sprintf "  GARBAGE RATE: 1/%d" interval) |> ignore
            else
                line.Append(sprintf "  LINES: %d/%d" state.LinesClearedInStage state.LinesRequiredForNextStage) |> ignore
        | 5 -> line.Append("  NEXT PIECE:") |> ignore
        | 6|7|8|9 -> 
            if not (state.ActiveModifiers |> List.contains FogOfWar) then
                line.Append("  ") |> ignore
                let next = getOffsets state.NextPiece.Shape 0
                for px in 0 .. 3 do
                    if next |> List.exists (fun p -> p.X = px && p.Y = y-6) then
                        line.Append(colorCode state.NextPiece.Shape + "██" + reset + statsColor) |> ignore
                    else line.Append("  ") |> ignore
        | 11 -> line.Append("  HOLD [C]/[H]:") |> ignore
        | 12 -> line.Append(sprintf "  [C]: %-10s" (match state.HoldSlot1 with Some p -> sprintf "%A" p.Shape | _ -> "Empty")) |> ignore
        | 13 -> line.Append(sprintf "  [H]: %-10s" (if state.HasExtraSlot then (match state.HoldSlot2 with Some p -> sprintf "%A" p.Shape | _ -> "Empty") else "LOCKED")) |> ignore
        | 15 -> line.Append("  POWER-UPS:") |> ignore
        | 16|17|18 -> 
            let idx = y - 16
            if idx < state.ActivePowerUps.Length then
                line.Append(sprintf "  - %-15A" state.ActivePowerUps.[idx]) |> ignore
        | 19 -> 
            if state.QuickClearCharges > 0 then
                line.Append(sprintf "  [Q] CLEAR: %d" state.QuickClearCharges) |> ignore
        | 21 -> line.Append("  MODIFIERS:") |> ignore
        | 22 -> 
            if state.Status = EndlessMode then
                line.Append("  ! FOG + GARBAGE ACTIVE") |> ignore
            else
                let mods = if state.ActiveModifiers.IsEmpty then "None" else state.ActiveModifiers |> List.map (sprintf "%A") |> String.concat ", "
                line.Append(sprintf "  ! %s" mods) |> ignore
        | _ -> ()

  
  
        sb.Append(line.ToString()).Append("                                                  \n") |> ignore

    Console.Write(sb.ToString())


  
let drawPowerUpSelection (options: PowerUp list) =
    Console.Clear()
    printfn "\n\n   === STAGE COMPLETE! ==="
    printfn "   Choose your power-up:\n"
    options |> List.iteri (fun i p -> 
        let desc = Progression.getPowerUpDescription p
        printfn "   %d. %-15A : %s" (i + 1) p desc)
    printfn "\n   Press [1], [2], or [3] to select."

let drawBetweenStagesMenu (state: GameState) =
    Console.Clear()
    let highlight = "\u001b[38;2;255;255;255m" 
    printfn "\n\n    === STAGE %d COMPLETE ===" state.Level
    printfn "    %sWhat would you like to do?%s" statsColor reset
    printfn "\n    [S] Save and Quit to Menu"
    
  
    if state.Level >= 5 && state.Status <> EndlessMode then
        printfn "    [E] %sENTER ENDLESS MODE%s (Modifiers will stack!)" highlight reset
    else
        printfn "    [N] Proceed to Next Stage"


let drawLeaderboard (scores: ScoreEntry list) =
    Console.Clear()
    printfn "\n   === LEADERBOARD ==="
    scores |> List.iteri (fun i s -> printfn "   %d. %s - %d" (i+1) s.PlayerName s.FinalScore)
    printfn "\n   Press any key to return..."
    Console.ReadKey() |> ignore
let drawReplacementMenu (active: PowerUp list) (newPower: PowerUp) =
    Console.Clear()
    let frame = "\u001b[38;2;255;100;100m"  
    let highlight = "\u001b[38;2;255;255;255m"
    
    printfn "\n\n    %s┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓%s" frame reset
    printfn "    ┃          %sPOWER-UP SLOTS FULL!%s          ┃" highlight reset
    printfn "    ┃    Choose which one to replace:        ┃" 
    printfn "    %s┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛%s" frame reset
    printfn ""

    printfn "    %sNEW POWER-UP:%s %A (%s)" highlight reset newPower (Progression.getPowerUpDescription newPower)
    printfn "\n    %sEXISTING SLOTS:%s" frame reset
    
    active |> List.iteri (fun i p ->
        printfn "      [%d] %-15A : %s" (i + 1) p (Progression.getPowerUpDescription p)
    )
    
    printfn "\n    [4] Skip and keep current power-ups."
    printfn "\n    Press 1, 2, 3 to Replace, or 4 to Skip."

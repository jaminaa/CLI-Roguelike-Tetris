namespace RoguelikeTetris

open System
open Domain
open UI
open GameLoop
open SaveSystem
open Progression

module Program =

  
    let createNewGame () =
        let (lines, speed) = getStageRequirements 1
        {
            Board = Board.emptyBoard
            CurrentPiece = Tetrominoes.spawnRandom()
            NextPiece = Tetrominoes.spawnRandom()
            Score = 0
            Level = 1
            LinesClearedInStage = 0
            LinesRequiredForNextStage = lines
            FallingSpeed = speed
            ActivePowerUps = []
            ActiveModifiers = []
            Status = Playing
            HoldSlot1 = None
            HoldSlot2 = None
            HasExtraSlot = false
            RemainingHolds = 1
            PiecesPlacedInStage = 0 
            QuickClearCharges=0

        }

  
    let rec mainMenu () =
        Console.Clear()
        Console.ForegroundColor <- ConsoleColor.Cyan
        printfn "=============================="
        printfn "    CLI ROGUELIKE TETRIS      "
        printfn "=============================="
        Console.ResetColor()
        
        let saveExists = (loadGame()).IsSome
        
        printfn "1. New Game"
        if saveExists then 
            printfn "2. Continue"
        else 
            Console.ForegroundColor <- ConsoleColor.DarkGray
            printfn "2. Continue (No Save)"
            Console.ResetColor()
            
        printfn "3. Leaderboard"
        printfn "4. Exit"
        printf "\nSelect an option: "

        match Console.ReadLine() with
        | "1" -> 
            let state = createNewGame()
            Async.RunSynchronously (runGame state DateTime.Now)
            mainMenu()  

        | "2" when saveExists ->
            match loadGame() with
            | Some state -> 
                Async.RunSynchronously (runGame state DateTime.Now)
            | None -> ()
            mainMenu()

        | "3" ->
            let scores = getLeaderboard()
            drawLeaderboard scores
            mainMenu()

        | "4" -> 
            printfn "Thanks for playing!"
            0  

        | _ -> 
            printfn "Invalid option, press any key..."
            Console.ReadKey() |> ignore
            mainMenu()

  
    [<EntryPoint>]
    let main argv =
  
        try
            if OperatingSystem.IsWindows() then
                Console.WindowHeight <- 30
                Console.WindowWidth <- 60
        with _ -> 
            printfn "Please maximize your terminal window for the best experience."
            Threading.Thread.Sleep(2000)

        Console.CursorVisible <- false
        Console.OutputEncoding <- System.Text.Encoding.UTF8
        mainMenu()
module RoguelikeTetris.Board

open Domain
open Tetrominoes

let Width = 10
let Height = 20

  
  
let emptyBoard : Board = 
    List.init Height (fun _ -> List.init Width (fun _ -> None))

  
  
let isValid (board: Board) (piece: Tetromino) =
    let blocks = getGlobalBlocks piece
    blocks |> List.forall (fun p ->
        p.X >= 0 && p.X < Width && 
        p.Y >= 0 && p.Y < Height && 
        board.[p.Y].[p.X] = None)

  
  
let placePiece (board: Board) (piece: Tetromino) : Board =
    let blocks = getGlobalBlocks piece
    board |> List.mapi (fun y row ->
        row |> List.mapi (fun x cell ->
  
            if blocks |> List.exists (fun p -> p.X = x && p.Y = y) then
                Some piece.Shape
            else cell
        )
    )

  
  
let clearLines (board: Board) : Board * int =
    let isRowFull row = List.forall Option.isSome row
    
  
    let remainingRows = board |> List.filter (fun row -> not (isRowFull row))
    let linesCleared = Height - remainingRows.Length
    
  
    let newEmptyRows = List.init linesCleared (fun _ -> List.init Width (fun _ -> None))
    
  
    (newEmptyRows @ remainingRows, linesCleared)

  
let getGhostPiece (board: Board) (piece: Tetromino) =
    let rec findBottom p =
        let next = move 0 1 p  
        if isValid board next then 
            findBottom next 
        else 
            p  
    findBottom piece

let addGarbage (board: Board) : Board =
    let rnd = System.Random()
    let holeIndex = rnd.Next(0, 10)
  
    let garbageRow = List.init 10 (fun i -> if i = holeIndex then None else Some T)
    
  
    (List.tail board) @ [garbageRow]

let useQuickClear (board: Board) (powerUpCount: int) : Board =
  
    let linesToClear = powerUpCount * 2
  
    let remainingRows = board |> List.take (20 - linesToClear)
  
    let emptyRows = List.init linesToClear (fun _ -> List.init 10 (fun _ -> None))
  
    emptyRows @ remainingRows

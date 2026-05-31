module RoguelikeTetris.Tetrominoes

open Domain

  
  
let getOffsets shape rotation =
    match shape, rotation % 4 with
    | I, 0 | I, 2 -> [{X=0; Y=1}; {X=1; Y=1}; {X=2; Y=1}; {X=3; Y=1}]
    | I, 1 | I, 3 -> [{X=2; Y=0}; {X=2; Y=1}; {X=2; Y=2}; {X=2; Y=3}]
    
    | O, _         -> [{X=1; Y=0}; {X=2; Y=0}; {X=1; Y=1}; {X=2; Y=1}]
    
    | T, 0 -> [{X=1; Y=0}; {X=0; Y=1}; {X=1; Y=1}; {X=2; Y=1}]
    | T, 1 -> [{X=1; Y=0}; {X=1; Y=1}; {X=2; Y=1}; {X=1; Y=2}]
    | T, 2 -> [{X=0; Y=1}; {X=1; Y=1}; {X=2; Y=1}; {X=1; Y=2}]
    | T, 3 -> [{X=1; Y=0}; {X=0; Y=1}; {X=1; Y=1}; {X=1; Y=2}]
    
    | S, 0 | S, 2 -> [{X=1; Y=0}; {X=2; Y=0}; {X=0; Y=1}; {X=1; Y=1}]
    | S, 1 | S, 3 -> [{X=1; Y=0}; {X=1; Y=1}; {X=2; Y=1}; {X=2; Y=2}]
    
    | Z, 0 | Z, 2 -> [{X=0; Y=0}; {X=1; Y=0}; {X=1; Y=1}; {X=2; Y=1}]
    | Z, 1 | Z, 3 -> [{X=2; Y=0}; {X=1; Y=1}; {X=2; Y=1}; {X=1; Y=2}]
    
    | J, 0 -> [{X=0; Y=0}; {X=0; Y=1}; {X=1; Y=1}; {X=2; Y=1}]
    | J, 1 -> [{X=1; Y=0}; {X=2; Y=0}; {X=1; Y=1}; {X=1; Y=2}]
    | J, 2 -> [{X=0; Y=1}; {X=1; Y=1}; {X=2; Y=1}; {X=2; Y=2}]
    | J, 3 -> [{X=1; Y=0}; {X=1; Y=1}; {X=0; Y=2}; {X=1; Y=2}]
    
    | L, 0 -> [{X=2; Y=0}; {X=0; Y=1}; {X=1; Y=1}; {X=2; Y=1}]
    | L, 1 -> [{X=1; Y=0}; {X=1; Y=1}; {X=1; Y=2}; {X=2; Y=2}]
    | L, 2 -> [{X=0; Y=1}; {X=1; Y=1}; {X=2; Y=1}; {X=0; Y=2}]
    | L, 3 -> [{X=0; Y=0}; {X=1; Y=0}; {X=1; Y=1}; {X=1; Y=2}]
    | _ -> []  

  
  
let getGlobalBlocks (piece: Tetromino) =
    getOffsets piece.Shape piece.Rotation
    |> List.map (fun offset -> 
        { X = piece.Position.X + offset.X; 
          Y = piece.Position.Y + offset.Y })

  
let move dx dy (piece: Tetromino) =
    { piece with Position = { X = piece.Position.X + dx; Y = piece.Position.Y + dy } }

  
let rotate clockwise (piece: Tetromino) =
    let newRotation = 
        if clockwise then (piece.Rotation + 1) % 4
        else (piece.Rotation + 3) % 4  
    { piece with Rotation = newRotation }

  
let random = System.Random()
let spawnRandom() =
    let shapes = [I; O; T; S; Z; J; L]
    let shape = shapes.[random.Next(shapes.Length)]
    { Shape = shape; Position = { X = 3; Y = 0 }; Rotation = 0 }

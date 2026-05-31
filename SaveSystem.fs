module RoguelikeTetris.SaveSystem

open System.IO
open System.Text.Json
open System.Text.Json.Serialization  
open Domain

  
let jsonOptions = JsonSerializerOptions()
jsonOptions.Converters.Add(JsonFSharpConverter())

let savePath = "savegame.json"
let leaderboardPath = "leaderboard.json"

  
let saveGame (state: GameState) : unit =
    let json = JsonSerializer.Serialize(state, jsonOptions)  
    File.WriteAllText(savePath, json)
    printfn "\nGame Saved Successfully!"

let loadGame () : GameState option =
    if File.Exists(savePath) then
        let json = File.ReadAllText(savePath)
        try
  
            Some(JsonSerializer.Deserialize<GameState>(json, jsonOptions))
        with _ -> None
    else
        None

  
let saveScore (entry: ScoreEntry) : unit =
    let existingScores = 
        if File.Exists(leaderboardPath) then
            let json = File.ReadAllText(leaderboardPath)
            JsonSerializer.Deserialize<ScoreEntry list>(json, jsonOptions)
        else []
    
    let updatedScores = 
        entry :: existingScores 
        |> List.sortByDescending (fun s -> s.FinalScore)
        |> List.truncate 10
    
    let json = JsonSerializer.Serialize(updatedScores, jsonOptions)
    File.WriteAllText(leaderboardPath, json)

let getLeaderboard () : ScoreEntry list =
    if File.Exists(leaderboardPath) then
        let json = File.ReadAllText(leaderboardPath)
        JsonSerializer.Deserialize<ScoreEntry list>(json, jsonOptions)
    else []
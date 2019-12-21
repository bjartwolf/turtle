module Pictures

open SharpDX.Direct2D1
open Boxes

// I do not know what this thing is yet
type PictureFoo = {
  turn: Picture -> Picture;
  flip: Picture -> Picture;
  toss: Picture -> Picture;
  over: Picture -> Picture -> Picture;
  beside: Picture -> Picture -> Picture;
  above: Picture -> Picture -> Picture;
}

let turn (p : Picture) : Picture = 
  turn >> p

let flip (p : Picture) : Picture = 
  flip >> p

let toss (p : Picture) : Picture = 
  toss >> p

let besideRatio (group: Geometry array -> GeometryGroup) (m : int) (n : int) (p1 : Picture) (p2: Picture) : Picture =
  fun box ->
    let factor = float32 m / float32 (m + n)
    let b1, b2 = splitVertically factor box
    group [|p1 b1; p2 b2|] :> Geometry 

let aboveRatio (group: Geometry array -> GeometryGroup) (m : int) (n : int) (p1 : Picture) (p2 : Picture) : Picture =
  fun box ->
    let factor = float32 m / float32 (m + n)
    let b1, b2 = splitHorizontally factor box
    group [|p1 b1; p2 b2|] :> Geometry 

let over (group: Geometry array -> GeometryGroup) (p1 : Picture) (p2: Picture) : Picture = 
  fun box ->
     group [|p1 box; p2 box|] :> Geometry 

let getPictureFoo (group: Geometry array -> GeometryGroup) : PictureFoo =
  {
    turn = turn;
    flip = flip;
    toss = toss; 
    over = over group ;
    beside = besideRatio group 1 1; 
    above =  aboveRatio group 1 1; 
  }
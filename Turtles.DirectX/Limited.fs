module Limited

open Pictures
open Boxes 
open SharpDX.Direct2D1

type Foo = {
  ttile: Picture -> Picture;
  utile: Picture -> Picture;
  toss: Picture -> Picture;
  turn: Picture -> Picture;
  quartet: Picture -> Picture -> Picture -> Picture -> Picture;
  squareLimit: int -> Picture -> Picture 
  flip: Picture -> Picture 
  over: Picture -> Picture -> Picture 
} 

let getThings (blankGeo: Geometry) (group: Geometry array -> GeometryGroup)  = 
  let bar = getPictureFoo group
  let flip = bar.flip 
  let toss = bar.toss 
  let over = bar.over 
  let above = bar.above
  let turn = bar.turn
  let beside= bar.beside

  let blank : Picture = 
    fun box -> blankGeo 

  let ttile f = 
     let fishN = f |> toss |> flip
     let fishE = fishN |> turn |> turn |> turn 
     over f (over fishN fishE)

  let utile f = 
    let fishN = f |> toss |> flip
    let fishW = fishN |> turn
    let fishS = fishW |> turn
    let fishE = fishS |> turn
    over (over fishN fishW)
         (over fishE fishS)

  let quartet p q r s = 
    above (beside p q) (beside r s)

  let rec side n p = 
    let s = if n = 1 then blank else side (n - 1) p
    let t = ttile p
    quartet s s (t |> turn) t

  let rec corner n p = 
    let c, s = if n = 1 then blank, blank 
               else corner (n - 1) p, side (n - 1) p
    let u = utile p
    quartet c s (s |> turn) u

  let nonet p q r s t u v w x = 
    aboveRatio group 1 2 (besideRatio group 1 2 p (beside q r))
                   (aboveRatio group 1 1 (besideRatio group 1 2 s (beside t u))
                                   (besideRatio group 1 2 v (beside w x)))

  let squareLimit n p =
    let cornerNW = corner n p
    let cornerSW = turn cornerNW
    let cornerSE = turn cornerSW
    let cornerNE = turn cornerSE
    let sideN = side n p
    let sideW = turn sideN
    let sideS = turn sideW
    let sideE = turn sideS
    let center = utile p
    nonet cornerNW sideN cornerNE  
          sideW center sideE
          cornerSW sideS cornerSE
  {
    ttile = ttile;
    utile = utile; 
    turn = turn; 
    toss = toss; 
    flip = flip; 
    over = over; 
    quartet = quartet;
    squareLimit = squareLimit; 
  }
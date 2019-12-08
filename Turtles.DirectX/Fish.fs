module Fishier
open SharpDX
open SharpDX.Mathematics
open SharpDX.Direct2D1

let createBezier (x1, y1) (x2, y2) (x3, y3) : BezierSegment = 
  let segment = BezierSegment()
  segment.Point1.X <- float32 x1 
  segment.Point1.Y <- float32 y1
  segment.Point2.X <- float32 x2
  segment.Point2.Y <- float32 y2
  segment.Point3.X <- float32 x3
  segment.Point3.Y <- float32 y3
  segment

let fishyBeziers = [|
  createBezier (0.110, 0.110) 
               (0.175, 0.175) 
               (0.250, 0.250)
  createBezier (0.372, 0.194) 
               (0.452, 0.132) 
               (0.564, 0.032)
  createBezier (0.730, 0.056) 
               (0.834, 0.042) 
               (1.000, 0.000)
  createBezier (0.896, 0.062) 
               (0.837, 0.107) 
               (0.766, 0.202)     
  createBezier (0.660, 0.208)
               (0.589, 0.217)
               (0.500, 0.250)
  createBezier (0.500, 0.410)
               (0.500, 0.460)
               (0.500, 0.500)
  createBezier (0.500, 0.575)
               (0.500, 0.625)
               (0.500, 0.750)
  createBezier (0.411, 0.783)
               (0.340, 0.792)
               (0.234, 0.798)
  createBezier (0.163, 0.893)
               (0.104, 0.938)
               (0.000, 1.000)
  createBezier (-0.042, 0.834)
               (-0.056, 0.730)
               (-0.032, 0.564)
  createBezier (-0.132, 0.452)
               (-0.194, 0.372)
               (-0.250, 0.250)
  createBezier (-0.150, 0.150)
               (-0.050, 0.050)
               (0.000, 0.000)
|]

let addFishToSink (sink: GeometrySink) : unit=
  sink.AddBeziers(fishyBeziers)
  ()

module Fish
open SharpDX
open SharpDX.Mathematics
open SharpDX.Direct2D1
open Boxes 

let createPoint (x1:float) (y1:float) =
  Vector2(float32 x1, float32 y1)

let createBezier (x1, y1) (x2, y2) (x3, y3) : BezierSegment = 
  let segment = BezierSegment()
  segment.Point1.X <- float32 x1 
  segment.Point1.Y <- float32 y1
  segment.Point2.X <- float32 x2
  segment.Point2.Y <- float32 y2
  segment.Point3.X <- float32 x3
  segment.Point3.Y <- float32 y3
  segment

type Curve = Vector * BezierSegment
let createCurve (start: Vector) 
                (p1: Vector ) 
                (p2: Vector ) 
                (p3: Vector ) =
  let p1' = Interop.RawVector2(p1.X, p1.Y)
  let p2' = Interop.RawVector2(p2.X, p2.Y)
  let p3' = Interop.RawVector2(p3.X, p3.Y)
  (start, BezierSegment(Point1 = p1', Point2 = p2', Point3 = p3'))

let hendersonFishCurves = [
    createCurve (createPoint 0.0 0.0)
                (createPoint 0.0 0.2)
                (createPoint 0.0 0.5)
                (createPoint 0.0 1.0)
  ; createCurve (createPoint 0.0 0.0)
                (createPoint 0.2 0.0)
                (createPoint 0.5 0.0)
                (createPoint 1.0 0.0)
  ; createCurve (createPoint 0.116 0.702)
                (createPoint 0.260 0.295)
                (createPoint 0.330 0.258)
                (createPoint 0.815 0.078)
  ; createCurve (createPoint 0.564 0.032)
                (createPoint 0.730 0.056)
                (createPoint 0.834 0.042)
                (createPoint 1.000 0.000)
  ; createCurve (createPoint 0.250 0.250)
                (createPoint 0.372 0.194)
                (createPoint 0.452 0.132)
                (createPoint 0.564 0.032)
  ; createCurve (createPoint 0.000 0.000)
                (createPoint 0.110 0.110)
                (createPoint 0.175 0.175)
                (createPoint 0.250 0.250)
  ; createCurve (createPoint -0.250 0.250)
                (createPoint -0.150 0.150)
                (createPoint -0.090 0.090)
                (createPoint 0.000 0.000)
  ; createCurve (createPoint -0.250 0.250)
                (createPoint -0.194 0.372)
                (createPoint -0.132 0.452)
                (createPoint -0.032 0.564)
  ; createCurve (createPoint -0.032 0.564)
                (createPoint 0.055 0.355)
                (createPoint 0.080 0.330)
                (createPoint 0.250 0.250)
  ; createCurve (createPoint -0.032 0.564)
                (createPoint -0.056 0.730)
                (createPoint -0.042 0.834)
                (createPoint 0.000 1.000)
  ; createCurve (createPoint 0.000 1.000)
                (createPoint 0.104 0.938)
                (createPoint 0.163 0.893)
                (createPoint 0.234 0.798)
  ; createCurve (createPoint 0.234 0.798)
                (createPoint 0.368 0.650)
                (createPoint 0.232 0.540)
                (createPoint 0.377 0.377)    
  ; createCurve (createPoint 0.377 0.377)
                (createPoint 0.400 0.350)
                (createPoint 0.450 0.300)
                (createPoint 0.500 0.250)    
  ; createCurve (createPoint 0.500 0.250)
                (createPoint 0.589 0.217)
                (createPoint 0.660 0.208)
                (createPoint 0.766 0.202)    
  ; createCurve (createPoint 0.766 0.202)
                (createPoint 0.837 0.107)
                (createPoint 0.896 0.062)
                (createPoint 1.000 0.000)
  ; createCurve (createPoint 0.234 0.798)
                (createPoint 0.340 0.792)
                (createPoint 0.411 0.783)
                (createPoint 0.500 0.750)
  ; createCurve (createPoint 0.500 0.750)
                (createPoint 0.500 0.625)
                (createPoint 0.500 0.575)
                (createPoint 0.500 0.500)
  ; createCurve (createPoint 0.500 0.500)
                (createPoint 0.460 0.460)
                (createPoint 0.410 0.410)
                (createPoint 0.377 0.377)
  ; createCurve (createPoint 0.315 0.710)
                (createPoint 0.378 0.732)
                (createPoint 0.426 0.726)
                (createPoint 0.487 0.692)
  ; createCurve (createPoint 0.340 0.605)
                (createPoint 0.400 0.642)
                (createPoint 0.435 0.647)
                (createPoint 0.489 0.626)
  ; createCurve (createPoint 0.348 0.502)
                (createPoint 0.400 0.564)
                (createPoint 0.422 0.568)
                (createPoint 0.489 0.563)
  ; createCurve (createPoint 0.451 0.418)
                (createPoint 0.465 0.400)
                (createPoint 0.480 0.385)
                (createPoint 0.490 0.381)
  ; createCurve (createPoint 0.421 0.388)
                (createPoint 0.440 0.350)
                (createPoint 0.455 0.335)
                (createPoint 0.492 0.325)
  ; createCurve (createPoint -0.170 0.237)
                (createPoint -0.125 0.355)
                (createPoint -0.065 0.405)
                (createPoint 0.002 0.436)
  ; createCurve (createPoint -0.121 0.188)
                (createPoint -0.060 0.300)
                (createPoint -0.030 0.330)
                (createPoint 0.040 0.375)
  ; createCurve (createPoint -0.058 0.125)
                (createPoint -0.010 0.240)
                (createPoint 0.030 0.280)
                (createPoint 0.100 0.321)
  ; createCurve (createPoint -0.022 0.063)
                (createPoint 0.060 0.200)
                (createPoint 0.100 0.240)
                (createPoint 0.160 0.282)
  ; createCurve (createPoint 0.053 0.658)
                (createPoint 0.075 0.677)
                (createPoint 0.085 0.687)
                (createPoint 0.098 0.700)
  ; createCurve (createPoint 0.053 0.658)
                (createPoint 0.042 0.710)
                (createPoint 0.042 0.760)
                (createPoint 0.053 0.819)
  ; createCurve (createPoint 0.053 0.819)
                (createPoint 0.085 0.812)
                (createPoint 0.092 0.752)
                (createPoint 0.098 0.700)
  ; createCurve (createPoint 0.130 0.718)
                (createPoint 0.150 0.730)
                (createPoint 0.175 0.745)
                (createPoint 0.187 0.752)
  ; createCurve (createPoint 0.130 0.718)
                (createPoint 0.110 0.795)
                (createPoint 0.110 0.810)
                (createPoint 0.112 0.845)
  ; createCurve (createPoint 0.112 0.845)
                (createPoint 0.150 0.805)
                (createPoint 0.172 0.780)
                (createPoint 0.187 0.752) ]

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

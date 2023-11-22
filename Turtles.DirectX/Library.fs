open System
open SharpDX
open SharpDX.Mathematics
open SharpDX.Direct2D1
open Boxes 
open Limited 

[<STAThread>]
[<EntryPoint>]
let main argv = 
    use d2DFactory = new Direct2D1.Factory()

    let matrixToRaw (mtrx: Matrix3x2) =
        Interop.RawMatrix3x2(mtrx.M11, mtrx.M12, mtrx.M21, mtrx.M22, mtrx.M31, mtrx.M32)

    let translate (translationx: float32) (translationy: float32)= 
        let center = Vector2(translationx, translationy)
        Matrix3x2.Translation(center)

    let scaleOrigo (scalex:float32) (scaley: float32) =
        Matrix3x2.Scaling(scalex, scaley)

    let rotate angle (point : Vector2) = Matrix3x2.Rotation(angle, point)
     
    let emptyGeo = new PathGeometry(d2DFactory)
    let emptySink = emptyGeo.Open()
    emptySink.Close()

    let fishGeo = new PathGeometry(d2DFactory) 
    let sink = fishGeo.Open()
    sink.Close()
    let mutable matrixes: Matrix3x2 ResizeArray = ResizeArray()
    let boxToMtrx (box: Box) : Matrix3x2 = 
        let bLength = box.b.Length()
        let cLength = box.c.Length()
        let delta = box.c.X * box.b.Y - box.c.Y*box.b.X 
        let cScale = if delta < 0.0f then
                        cLength
                     else 
                        -cLength

        let rotAngleB = float32 (Math.Atan2(float box.b.Y, float box.b.X))
        let mtrx = (scaleOrigo bLength cScale) * (translate box.a.X box.a.Y) * (rotate rotAngleB box.a)
        matrixes.Add(mtrx)
        mtrx

    let grouper (factory: Direct2D1.Factory) (geos: Geometry []) = 
        new GeometryGroup(factory, FillMode.Alternate, geos)

    let transform : Box -> Geometry -> Geometry = 
      let transformer (factory: Direct2D1.Factory) (box: Box) (geo : Geometry) : Geometry =
        let mtrx = boxToMtrx box
        new TransformedGeometry(factory, geo, mtrx |> matrixToRaw) :> Geometry
      transformer d2DFactory 

    let group = grouper d2DFactory 
    let baz = getThings emptyGeo group 

    let fish = fishGeo :> Geometry 
    let b: Box =  { a = Vector(0.0f, 0.0f); 
                    b = Vector(1500.0f, 000.0f);
                    c = Vector(0.0f, 1500.0f)}

    let f = fun (box:Box) -> transform box fish 
    let pic : Geometry = baz.squareLimit 5 f b

    let matrixToArrays (matrix: Matrix3x2): float32 array array =
        [|
        [| matrix.M11; matrix.M12; float32 0.0 |];
        [| matrix.M21; matrix.M22; float32 0.0 |];
        [| matrix.M31; matrix.M32; float32 0.0 |]
        |]
    let allMatrixes = Seq.map (fun x -> matrixToArrays x) matrixes |> Seq.toList

    printfn "%A" (allMatrixes.[1])
    printfn "%A" (allMatrixes.Length)

    printfn "%A" argv
//    device.ClearState()
//    device.Flush()
//    device.Dispose()
//    (!swapChain).Dispose()
    0 // return an integer exit code
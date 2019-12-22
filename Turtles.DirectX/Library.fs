open System
open System.Drawing
open System.Windows
open SharpDX
open SharpDX.Mathematics
open SharpDX.Direct2D1
open SharpDX.Direct3D10
open SharpDX.DXGI
open SharpDX.Windows
open Fish
open Boxes 
open Limited 
open ScreenSettings

open My.Turtles
open SharpDX.Direct2D1.Effects

[<STAThread>]
[<EntryPoint>]
let main argv = 
    let form = new RenderForm("Fish", Size = Size(ScreenRes.x_max, ScreenRes.y_max))

    let desc = SwapChainDescription (
                BufferCount = 1,
                ModeDescription =
                    ModeDescription(form.ClientSize.Width, form.ClientSize.Height,
                        Rational(60, 1), Format.R8G8B8A8_UNorm),
                OutputHandle = form.Handle,
                SampleDescription = SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                IsWindowed = Interop.RawBool(true),
                Usage = Usage.RenderTargetOutput)
    let mutable device: SharpDX.Direct3D10.Device1 = null
    let swapChain = ref null 
    SharpDX.Direct3D10.Device1.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport, desc,
                SharpDX.Direct3D10.FeatureLevel.Level_10_1, &device, swapChain) 

    use d2DFactory = new Direct2D1.Factory()
    use factory = (!swapChain).GetParent<Factory>()
    factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll)
    use backBuffer = Resource.FromSwapChain<Texture2D>((!swapChain), 0)
    use renderTrgt = new RenderTargetView(device, backBuffer)
    let surface = backBuffer.QueryInterface<Surface>()
    let d2DRenderTarget = new RenderTarget(d2DFactory, surface, 
                                  RenderTargetProperties(
                                    PixelFormat(Format.Unknown, Direct2D1.AlphaMode.Premultiplied)))

    let hotpink = Color.HotPink.ToVector3()
    
    let pink = Interop.RawColor4(hotpink.X, hotpink.Y, hotpink.Z, 50.0f)
    
    let pinkBrush = new SolidColorBrush(d2DRenderTarget, pink, BrushProperties(Opacity = 1.00f) |> Nullable<BrushProperties>)

    let matrixToRaw (mtrx: Matrix3x2) =
        Interop.RawMatrix3x2(mtrx.M11, mtrx.M12, mtrx.M21, mtrx.M22, mtrx.M31, mtrx.M32)

    let translate (translationx: float32) (translationy: float32)= 
        let center = Vector2(translationx, translationy)
        Matrix3x2.Translation(center)

    let scaleOrigo (scalex:float32) (scaley: float32) =
        Matrix3x2.Scaling(scalex, scaley)

    let scale (scalex:float32) (scaley: float32) (center: Vector2)= 
        Matrix3x2.Scaling(scalex, scaley, center)

    ///<summary>Missing rotation point?</summary>
    let skew(angleX:float32) (angleY: float32) = 
        Matrix3x2.Skew(angleX, angleY)

    let rotate angle (point : Vector2) = Matrix3x2.Rotation(angle, point)
     
    let emptyGeo = new PathGeometry(d2DFactory)
    let emptySink = emptyGeo.Open()
    emptySink.Close()

    let fishGeo = new PathGeometry(d2DFactory)
    let sink = fishGeo.Open()
    sink.SetFillMode(Direct2D1.FillMode.Alternate)
    for (start, bezierCurve) in hendersonFishCurves do
        let start = Interop.RawVector2(start.X, start.Y)
        sink.BeginFigure(start, FigureBegin.Hollow  )
        sink.AddBezier(bezierCurve)
        sink.EndFigure(FigureEnd.Open)
    let foo = sink.Close()

    let boxToMtrx (box: Box) : Matrix3x2 = 
        let bLength = box.b.Length()
        let cLength = box.c.Length()
        let dotProdBC = Vector2.Dot(box.b,box.c)
        let angleBC = acos (dotProdBC / (bLength * cLength )) 
        let cScale = if float angleBC >= Math.PI || float angleBC <= -Math.PI then
                        -cLength
                     else 
                        cLength

        let rotAngleB = float32 (Math.Atan2(float box.b.Y, float box.b.X))
        Console.WriteLine(sprintf "Vector a: %A Vector b: %A Vector c: %A " box.a box.b box.c)
        Console.WriteLine(sprintf "C-skalering: %A   - Vinkel B og C vektor: %f" cScale angleBC)
//        transform (rotate rotAngleB box.a) geo
        //transform ((rotate rotAngleB box.a) * (scale bLength cLength box.a) * (translate box.a.X box.a.Y)) geo
        let mtrx = (scaleOrigo bLength cScale) * (translate box.a.X box.a.Y) * (rotate rotAngleB box.a)
        mtrx

    let grouper (factory: Direct2D1.Factory) (geos: Geometry []) = 
        new GeometryGroup(factory, FillMode.Alternate, geos)

    let geoInBox (transform: Box -> Geometry -> Geometry) (geo: Geometry) (box: Box) : Geometry =
        // kan droppe skew hvis vi ikke trenger det...
        // roterer bare etter B aksen og skalerer motsatt om c vektoren peker i en annen retning. kan 
        // lager en matrix3x2 av boxen
        transform box geo

//    let bmp : Bitmap = new Bitmap()
//    let bitmapBrush = new BitmapBrush(d2DRenderTarget, LoadBitmap.Load "image.jpg" d2DRenderTarget)
    let draw (geo: Geometry) =
        //d2DRenderTarget.DrawGeometry(geo, pinkBrush, strokeWidth = 1.0f)
        d2DRenderTarget.DrawGeometry(geo, pinkBrush)
//        d2DRenderTarget.FillGeometry(geo, bitmapBrush)

    /// Her ligger nok litt av trikset
    /// Må bruke denne også i kombinasjoner...?
    let transform : Box -> Geometry -> Geometry = 
      let transformer (factory: Direct2D1.Factory) (box: Box) (geo : Geometry) : Geometry =
        let mtrx = boxToMtrx box
        new TransformedGeometry(factory, geo, mtrx |> matrixToRaw) :> Geometry
      transformer d2DFactory 

    let group = grouper d2DFactory 
    let baz = getThings emptyGeo group 

    let f : Picture = geoInBox transform fishGeo 
    let q = baz.flip f 
    RenderLoop.Run(form, fun _ ->
            //d2DRenderTarget.Clear(new Nullable<Interop.RawColor4>(Interop.RawColor4(0.0f, 0.0f, 0.0f, 0.90f)))
            d2DRenderTarget.BeginDraw()
            let b =  { a = Vector(300.0f, 300.0f); 
                       b = Vector(500.0f, 100.0f);
                       c = Vector(-100.0f, 500.0f)}
            draw (b |> (baz.over f q ))
            d2DRenderTarget.EndDraw()
            (!swapChain).Present(0, PresentFlags.None) |> ignore
        )
    printfn "%A" argv
    backBuffer.Dispose()
    device.ClearState()
    device.Flush()
    device.Dispose()
    (!swapChain).Dispose()
    0 // return an integer exit code
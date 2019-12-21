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
    let scale (scalex:float32) (scaley: float32)= 
        let scale = Vector2(scalex, scaley)
        Matrix3x2.Scaling(scale)


    ///<summary>Missing rotation point?</summary>
    let skew(angleX:float32) (angleY: float32) = 
        Matrix3x2.Skew(angleX, angleY)

    ///<summary>Flips around the x-axis</summary>
    let flip = 
        scale 1.0f -1.0f

    let rotate angle = Matrix3x2.Rotation(angle)
     
    let emptyGeo = new PathGeometry(d2DFactory)
    let emptySink = emptyGeo.Open()
    emptySink.Close()

    let fishGeo = new PathGeometry(d2DFactory)
    let sink = fishGeo.Open()
    sink.SetFillMode(Direct2D1.FillMode.Winding)
    for (start, bezierCurve) in hendersonFishCurves do
        let start = Interop.RawVector2(start.X, start.Y)
        sink.BeginFigure(start, FigureBegin.Filled)
        sink.AddBezier(bezierCurve)
        sink.EndFigure(FigureEnd.Closed)
    let foo = sink.Close()

    let transformer (factory: Direct2D1.Factory) (mtrx: Matrix3x2) (geo : Geometry) : Geometry =
        new TransformedGeometry(factory, geo, mtrx |> matrixToRaw) :> Geometry
    
    let grouper (factory: Direct2D1.Factory) (geos: Geometry []) = 
        new GeometryGroup(factory, FillMode.Alternate, geos)
    
    let group = grouper d2DFactory 

    let transform : Matrix3x2 -> Geometry -> Geometry = transformer d2DFactory 

    let geoInBox (geo: Geometry) (box: Box) : Geometry =
        // kan droppe skew hvis vi ikke trenger det...
        // roterer bare etter B aksen
        let bLength = box.b.Length()
        let cLength = box.c.Length()
        let rotAngleB = float32 (Math.Atan2(float box.b.Y, float box.b.X))
//        Console.WriteLine(sprintf "%A" rotAngleB)
        transform ((scale bLength cLength) * (rotate rotAngleB) * (translate box.a.X box.a.Y)) geo

//    let bmp : Bitmap = new Bitmap()
    let bitmapBrush = new BitmapBrush(d2DRenderTarget, LoadBitmap.Load "image.jpg" d2DRenderTarget)
    let draw (geo: Geometry) =
        //d2DRenderTarget.DrawGeometry(geo, pinkBrush, strokeWidth = 1.0f)
//        d2DRenderTarget.DrawGeometry(geo, bitmapBrush)
        d2DRenderTarget.FillGeometry(geo, bitmapBrush)

    let baz = getThings emptyGeo group 

    let f = geoInBox fishGeo 
//    let p = Boxes.translate (Vector2(0.0f,0.0f))
    let q = baz.utile f
//    let q = baz.squareLimit 9 f
 //   let q = baz.squareLimit 3 f
    let r = baz.quartet q q q q  

// x her går riktig vei og i riktig pixler
// y går ned. må flippe
// vil ha origo i mitten
//    d2DRenderTarget.Transform <- flip 
//                               * translate (float32 ScreenRes.x_max / 2.0f) (float32 -ScreenRes.y_max) 
//                               |> matrixToRaw
    RenderLoop.Run(form, fun _ ->
            //d2DRenderTarget.Clear(new Nullable<Interop.RawColor4>(Interop.RawColor4(0.0f, 0.0f, 0.0f, 0.90f)))
            d2DRenderTarget.BeginDraw()
            let b =  { a = Vector(50.0f, 50.0f); 
                       b = Vector(900.0f, 50.0f);
                       c = Vector(50.0f, 900.0f)}
            draw (b |> r)
//            draw (b |> f)
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
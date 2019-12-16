open System
open System.Drawing
open SharpDX
open SharpDX.Mathematics
open SharpDX.Direct2D1
open SharpDX.Direct3D10
open SharpDX.DXGI
open SharpDX.Windows
open Fish
open Boxes 
open Limited 

open My.Turtles
open SharpDX.Direct2D1.Effects

[<STAThread>]
[<EntryPoint>]
let main argv = 
    // 24 x 15
    let form = new RenderForm("Fish", Size = Size(1200, 1200))

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
    
    let pinkBrush = new SolidColorBrush(d2DRenderTarget, pink, BrushProperties(Opacity = 0.02f) |> Nullable<BrushProperties>)

    let matrixToRaw (mtrx: Matrix3x2) =
        Interop.RawMatrix3x2(mtrx.M11, mtrx.M12, mtrx.M21, mtrx.M22, mtrx.M31, mtrx.M32)

    let translate (translationx: float32) (translationy: float32)= 
        let center = Vector2(translationx, translationy)
        Matrix3x2.Translation(center)
    let scale (scalex:float32) (scaley: float32)= 
        let scale = Vector2(scalex, scaley)
        Matrix3x2.Scaling(scale)
    
    let fishGeo = new PathGeometry(d2DFactory)
    let sink = fishGeo.Open()
    for (start, bezierCurve) in hendersonFishCurves do
        let start = Interop.RawVector2(start.X, start.Y)
        sink.BeginFigure(start, FigureBegin.Hollow)
        sink.AddBezier(bezierCurve)
        sink.EndFigure(FigureEnd.Open)
    let foo = sink.Close()

    let drawPicture (picture: Picture) (box: Box)= 
        d2DRenderTarget.DrawGeometry(picture box, pinkBrush, strokeWidth = 1.0f)
    
    let transformer (factory: Direct2D1.Factory) (mtrx: Matrix3x2) (geo : Geometry) : Geometry =
        new TransformedGeometry(factory, geo, mtrx |> matrixToRaw) :> Geometry

    let grouper (factory: Direct2D1.Factory) (geos: Geometry []) = 
        new GeometryGroup (factory, Direct2D1.FillMode.Alternate, geos)
    
    let deg90 = (float32 Math.PI)/2.0f
    RenderLoop.Run(form, fun _ ->
            //d2DRenderTarget.Clear(new Nullable<Interop.RawColor4>(Interop.RawColor4(0.0f, 0.0f, 0.0f, 0.90f)))
            d2DRenderTarget.BeginDraw()
//            d2DRenderTarget.Transform <- translate 1000.0f 400.0f |> matrixToRaw
            let group = grouper d2DFactory 

            let createBox size =  
                { a = Vector(0.0f, 0.0f); 
                  b = Vector(size, 0.0f); 
                  c = Vector(0.0f, size)}
            let box1000 = createBox 1000.0f 
            let box100 = createBox 100.0f 
            let box10 = createBox 10.0f 
            let box1 = createBox 1.0f 

            let geoInBox (geo: Geometry) (box: Box) : Geometry =
                let transform : Matrix3x2 -> Geometry -> Geometry = transformer d2DFactory 
                transform ((scale box.b.X box.c.Y) * (translate box.a.X box.a.Y)) geo
   
            let draw (geo: Geometry) =
                d2DRenderTarget.DrawGeometry(geo, pinkBrush, strokeWidth = 1.0f)

            let baz = getThings group 

            let f = geoInBox fishGeo
//            let q = baz.squareLimit 4 f
//            let tile = baz.ttile (geoInBox' fishGeo) 
            let baar = baz.utile f 
            draw (baar box1000)

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
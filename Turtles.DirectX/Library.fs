open System
open System.Drawing
open SharpDX
open SharpDX.Mathematics
open SharpDX.Direct2D1
open SharpDX.Direct3D10
open SharpDX.DXGI
open SharpDX.Windows
open Fishier

open My.Turtles
open SharpDX.Direct2D1.Effects

[<STAThread>]
[<EntryPoint>]
let main argv = 
    // 24 x 15
    let form = new RenderForm("Fish", Size = Size(2000, 800))

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

    let rotate (angleInRads:float32) = 
         Matrix3x2.Rotation(angleInRads)
    let translate (translationx: float32) (translationy: float32)= 
        let center = Vector2(translationx, translationy)
        Matrix3x2.Translation(center)
    let scale (scale:float32) = 
        let scale = Vector2(scale, scale)
        Matrix3x2.Scaling(scale)
    let verticalFlip = 
        let scale = Vector2(-1.0f, 1.0f)
        Matrix3x2.Scaling(scale)
    let horizontalFlip = 
        let scale = Vector2(1.0f, -1.0f)
        Matrix3x2.Scaling(scale)
     
    let geo = new PathGeometry(d2DFactory)
    let sink = geo.Open()
    for (start, bezierCurve) in hendersonFishCurves do
        sink.BeginFigure(start, FigureBegin.Hollow)
        sink.AddBezier(bezierCurve)
        sink.EndFigure(FigureEnd.Open)
    let foo = sink.Close()
    let drawFish geo = d2DRenderTarget.DrawGeometry(geo, pinkBrush, strokeWidth = 1.0f)
    RenderLoop.Run(form, fun _ ->
            d2DRenderTarget.BeginDraw()
            d2DRenderTarget.Transform <- translate 1000.0f 400.0f |> matrixToRaw
            let fishSize = 100.0f
            //d2DRenderTarget.Clear(new Nullable<Interop.RawColor4>(Interop.RawColor4(0.0f, 0.0f, 0.0f, 0.90f)))
            let largerFish = new TransformedGeometry(d2DFactory, geo, (scale fishSize |> matrixToRaw)) 

            let rotFish  = new TransformedGeometry(d2DFactory, largerFish, (rotate (float32 (Math.PI/2.0))
                                                                          * translate fishSize fishSize |> matrixToRaw))

            let flipFish2 = new TransformedGeometry(d2DFactory, largerFish, (horizontalFlip
                                                                           * verticalFlip
                                                                           * translate fishSize fishSize |> matrixToRaw)) 

            let flipFish3 = new TransformedGeometry(d2DFactory, largerFish, (verticalFlip |> matrixToRaw)) 
            let flipFish4 = new TransformedGeometry(d2DFactory, largerFish, (horizontalFlip |> matrixToRaw)) 

            drawFish largerFish 
            drawFish rotFish
            drawFish flipFish2
            drawFish flipFish3
            drawFish flipFish4
            let group = new GeometryGroup(d2DFactory, Direct2D1.FillMode.Alternate, [|largerFish;
                                                                                      rotFish;
                                                                                      flipFish2;
                                                                                      flipFish3|])   
            let movedGroup = new TransformedGeometry(d2DFactory, group, translate fishSize fishSize |> matrixToRaw)  

            drawFish movedGroup 
//            drawFish flipFish2

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
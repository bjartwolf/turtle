﻿open System
open System.Drawing
open SharpDX
open SharpDX.Mathematics
open SharpDX.Direct2D1
open SharpDX.Direct3D11
open SharpDX.DXGI
open SharpDX.Windows
open Fish
open Boxes 
open Limited 
open ScreenSettings
open SharpDX.D3DCompiler
open SharpDX.Direct3D

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

    let mutable device: Direct3D11.Device = null

    let mutable swapChain: SwapChain = null 

    let driverType: DriverType = Direct3D.DriverType.Hardware

    SharpDX.Direct3D11.Device1.CreateWithSwapChain(driverType = driverType, flags = DeviceCreationFlags.BgraSupport, swapChainDescription = desc, device = &device, swapChain = &swapChain) 

    use d2DFactory = new Direct2D1.Factory()

    use factory = swapChain.GetParent<Factory>()
    factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll)
    use backBuffer = Resource.FromSwapChain<Texture2D>(swapChain, 0)
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

    sink.Close()
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
        mtrx

    let grouper (factory: Direct2D1.Factory) (geos: Geometry []) = 
        new GeometryGroup(factory, FillMode.Alternate, geos)

    let draw (geo: Geometry) = d2DRenderTarget.DrawGeometry(geo, pinkBrush, 0.5f)

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
    let pic : Geometry = baz.squareLimit 3 (f ) b

    let vertexShader = new ShaderBytecode(SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile("shaders.hlsl", "VSMain", "vs_5_0", SharpDX.D3DCompiler.ShaderFlags.Debug));
    //let vertexShader = new ShaderBytecode(SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile("shaders.hlsl", "VSMain", "vs_4_0", SharpDX.D3DCompiler.ShaderFlags.Debug));

    let vertexShaderDevice = new VertexShader(device, vertexShader.Data);

//    device.VertexShader.Set(vertexShaderDevice)
    //let inputElements = [| new InputElement("POSITION",0,Format.R32G32B32_Float,0,0) |]
    //device.InputAssembler.SetVertexBuffers()
    //device.InputAssembler


    RenderLoop.Run(form, fun _ ->
            d2DRenderTarget.BeginDraw()
            d2DRenderTarget.Clear(new Nullable<Interop.RawColor4>(Interop.RawColor4(0.0f, 0.0f, 0.0f, 1.010f)))
            draw pic
            d2DRenderTarget.EndDraw()
            swapChain.Present(0, PresentFlags.None) |> ignore
        )
    printfn "%A" argv
    backBuffer.Dispose()
//    device.ClearState()
//    device.Flush()
//    device.Dispose()
//    (!swapChain).Dispose()
    0 // return an integer exit code
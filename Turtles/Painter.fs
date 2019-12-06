module Painter

type point = double*double
type line = point*point
type PainterMsg = 
    | Line of line 

let painter =
    // Create a form to display the graphics
//    let width, height = 500, 500         
//    let form = new Form(Width = width, Height = height)
//    let box = new PictureBox(BackColor = Color.White, Dock = DockStyle.Fill)
//    let bmp = new Bitmap(1000,1000) 
//    let blackPen = new Pen(Color.Black, float32 3.0)
//    let graphics = Graphics.FromImage(bmp) 
//    box.Image <- bmp 
//    form.Controls.Add(box) 
////    form.ShowDialog() |> ignore
//    form.ShowDialog() |> ignore
    MailboxProcessor<PainterMsg>.Start(fun inbox -> 
                let rec loop n =
                    async { let! msg = inbox.Receive()
                            match msg with 
                                | Line(point)  -> let (p1,p2) = point 
                                                  let (x1,y1) = p1
                                                  let (x2,y2) = p2
                                                  let x1 = float32 x1 
                                                  let y1 = float32 y1 
                                                  let x2 = float32 x2 
                                                  let y2 = float32 y2 
                                                  return! loop 0}
                loop 0)
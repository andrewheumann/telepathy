using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Parameters;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Grasshopper.Kernel.Attributes;

namespace Telepathy
{
    // This class overrides the default display behavior of the params, to get the blue capsule
    // appearance and the little "Arrow" icons on the comp.

    public class RemoteParamAttributes : GH_FloatingParamAttributes
    {

        private System.Drawing.Rectangle m_textBounds; //maintain a rectangle of the text bounds
        private GH_StateTagList m_stateTags; //state tags are like flatten/graft etc.

        //handles state tag tooltips
        public override void SetupTooltip(PointF point, GH_TooltipDisplayEventArgs e)
        {
            if (this.m_stateTags != null)
            {
                this.m_stateTags.TooltipSetup(point, e);
                if (e.Valid)
                {
                    return;
                }
            }
            base.SetupTooltip(point, e);
        }


        //This method figures out the size and shape of elements.
        protected override void Layout()
        {
            //establish the size based on the text content
            float textWidth = (float)System.Math.Max(GH_FontServer.MeasureString(this.Owner.NickName, GH_FontServer.StandardBold).Width + 10, 50);
            System.Drawing.RectangleF bounds = new System.Drawing.RectangleF(this.Pivot.X - 0.5f * textWidth, this.Pivot.Y - 10f, textWidth, 20f);
            this.Bounds = bounds;
            this.Bounds = GH_Convert.ToRectangle(this.Bounds);

            this.m_textBounds = GH_Convert.ToRectangle(this.Bounds);

            // make space for the state tags, if any
            this.m_stateTags = this.Owner.StateTags;
            if (this.m_stateTags.Count == 0)
            {
                this.m_stateTags = null;
            }
            if (this.m_stateTags != null)
            {
                this.m_stateTags.Layout(GH_Convert.ToRectangle(this.Bounds), GH_StateTagLayoutDirection.Left);
                System.Drawing.Rectangle tag_box = this.m_stateTags.BoundingBox;
                if (!tag_box.IsEmpty)
                {
                    tag_box.Inflate(3, 0);
                    this.Bounds = System.Drawing.RectangleF.Union(this.Bounds, tag_box);
                }
            }

            // make space for the arrow
            if (Owner is Param_RemoteSender)
            {
                RectangleF arrowRect = new RectangleF(this.Bounds.Right, this.Bounds.Bottom, 10, 1);
                this.Bounds = RectangleF.Union(this.Bounds, arrowRect);
            }
            if (Owner is Param_RemoteReceiver)
            {
                RectangleF arrowRect = new RectangleF(this.Bounds.Left - 15, this.Bounds.Bottom, 15, 1);
                this.Bounds = RectangleF.Union(this.Bounds, arrowRect);
            }
        }

        //empty constructor 
        public RemoteParamAttributes(Param_GenericObject owner)
            : base(owner)
        {

        }

        //This method actually draws the parts
        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            //Draw the wires normally
            if (channel == GH_CanvasChannel.Wires)
            {
                if (Owner.SourceCount > 0)
                {
                    base.RenderIncomingWires(canvas.Painter, base.Owner.Sources, base.Owner.WireDisplay);
                }
            }
            //Draw the capsule and decorations
            if (channel == GH_CanvasChannel.Objects)
            {
                // call the default drawing routines
                base.Render(canvas, graphics, channel);

                //check if the capsule can be seen in the current viewport
                GH_Viewport viewport = canvas.Viewport;
                RectangleF bounds = this.Bounds;
                bool isVisible = viewport.IsVisible(ref bounds, 10f);
                this.Bounds = bounds;

                //if it can be seen...
                if (isVisible)
                {
                    // create a "text capsule"
                    // using statement makes sure it's properly disposed of (not 100% sure this is necessary)
                    using (GH_Capsule capsule = GH_Capsule.CreateTextCapsule(this.Bounds, m_textBounds, GH_Palette.Blue, Owner.NickName))
                    {

                        //draw the input and output "grip" bubbles
                        capsule.AddInputGrip(this.InputGrip.Y);
                        capsule.AddOutputGrip(this.OutputGrip.Y);
                        // render the capsule
                        bool hidden = (this.Owner as Param_GenericObject).Hidden;
                        capsule.Render(graphics, this.Selected, this.Owner.Locked, hidden);
                    }


                    if (this.m_stateTags != null)
                    {
                        this.m_stateTags.RenderStateTags(graphics);
                    }

                    //figure out where to draw the arrow
                    PointF arrowLocation = PointF.Empty;
                    if (Owner is Param_RemoteReceiver)
                    {
                        arrowLocation = new PointF(bounds.Left + 10, this.OutputGrip.Y + 2);
                    }
                    if (Owner is Param_RemoteSender)
                    {
                        arrowLocation = new PointF(bounds.Right - 10, this.OutputGrip.Y + 2);
                    }
                    //draw the arrow
                    if (arrowLocation != PointF.Empty) renderArrow(canvas, graphics, arrowLocation);
                }

            }

        }

        //Draws the arrow as a wingding text. Using text means the icon can be vector and look good zoomed in.
        private static void renderArrow(GH_Canvas canvas, Graphics graphics, PointF loc)
        {
            //Wingdings 3 has a nice arrow in the "g" char.

            //  Font font = new Font("Wingdings 3", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            //render the text at specified location
            //Version for everyone:
            GH_GraphicsUtil.RenderCenteredText(graphics, "\u2192", new Font("Arial", 10F), Color.Black, new PointF(loc.X, loc.Y - 1.5f));
            //Version for Marc:
           // GH_GraphicsUtil.RenderCenteredText(graphics, "*", new Font("Arial", 10F), Color.Black, loc);
        }


        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (Owner is Param_RemoteSender)
            {
                var ghDoc = Owner.OnPingDocument();

                var newParam = new Param_RemoteReceiver();

                ghDoc.AddObject(newParam, true, ghDoc.ObjectCount);
                newParam.NickName = Owner.NickName;
                newParam.Attributes.Pivot = new PointF(Pivot.X + 100, Pivot.Y);
                newParam.Attributes.ExpireLayout();
            }
            return base.RespondToMouseDoubleClick(sender, e);
        }
    }
}

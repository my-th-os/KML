using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace KML
{
    /// <summary>
    /// This class represents grid koords, which are integers.
    /// In opposite to Points, where X and Y are double and used for drawing positions.
    /// </summary>
    class IntPair
    {
        /// <summary>
        /// The X koord in the grid
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// The Y koord in the grid
        /// </summary>
        public int Y { get; private set; }

        /// <summary>
        /// Create a new IntPair with X and Y koords
        /// </summary>
        /// <param name="x">The X koord in the grid</param>
        /// <param name="y">The Y koord in the grid</param>
        public IntPair(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    /// <summary>
    /// A GuiVesselsPartGraph is linked to a Canvas element, 
    /// a graph will be drawn in to display all parts of a vessel and their connections.
    /// </summary>
    class GuiVesselsPartGraph
    {
        private Canvas VesselsDetails { get; set;}
        private Brush ConnectVerticalBrush { get; set; }
        private Brush ConnectSidesBrush { get; set; }
        private Brush ConnectSurfaceBrush { get; set; }
        private Brush ConnectDockBrush { get; set; }

        private List<List<GuiVesselsPartGraphNode>> PartGrid { get; set; }
        private int PartGridMinX { get; set; }
        private int PartGridMinY { get; set; }
        private int PartGridMaxX { get; set; }
        private int PartGridMaxY { get; set; }

        private GuiTabsManager Master { get; set; }

        private const double ElementWidth = 48;
        private const double ElementHeight = 32;

        /// <summary>
        /// Creates a GuiVesselsPartGraph to draw in the given Canvas.
        /// The master GuiTabsManager is needed to switch view on double click of a part.
        /// </summary>
        /// <param name="vesselsDetails">The Canvas to draw in</param>
        /// <param name="master">The master GuiTabsManager</param>
        public GuiVesselsPartGraph(Canvas vesselsDetails, GuiTabsManager master)
        {
            VesselsDetails = vesselsDetails;
            Master = master;
            ConnectVerticalBrush = new SolidColorBrush(Colors.Green);
            ConnectVerticalBrush.Freeze();
            ConnectSidesBrush = new SolidColorBrush(Colors.LimeGreen);
            ConnectSidesBrush.Freeze();
            ConnectSurfaceBrush = new SolidColorBrush(Colors.DarkGoldenrod);
            ConnectSurfaceBrush.Freeze();
            ConnectDockBrush = new SolidColorBrush(Colors.Blue);
            ConnectDockBrush.Freeze();
            InitGrid();
        }

        /// <summary>
        /// Draws all parts of the given KmlVessel.
        /// </summary>
        /// <param name="vessel">The KmlVessel to read all parts from</param>
        public void DrawPartStructure(KmlVessel vessel)
        {
            VesselsDetails.Children.Clear();
            InitGrid();
            if (vessel != null)
            {
                foreach (KmlPart part in vessel.Parts)
                {
                    part.Visited = false;
                }

                IntPair rootKoords = CalcKoords(new IntPair(0, 0), 0, 0);
                GuiVesselsPartGraphNode rootNode = DrawPart(vessel.RootPart, rootKoords);
                DrawConnectedParts(rootNode, rootKoords);

                // Draw all parts, that are not drawn yet
                foreach (KmlPart part in vessel.Parts)
                {
                    if (!part.Visited)
                    {
                        IntPair koords = CalcKoords(new IntPair(0, PartGridMaxY), 0, PartGridMaxY + CountTop(part, vessel.RootPart));
                        GuiVesselsPartGraphNode partNode = DrawPart(part, koords);
                        DrawConnectedParts(partNode, koords);
                    }
                }

                double minX = 0;
                double minY = 0;
                double maxX = 0;
                double maxY = 0;
                CalcMinMax(out minX, out minY, out maxX, out maxY);

                VesselsDetails.Width = maxX - minX + ElementWidth * 2;
                VesselsDetails.Height = maxY - minY + ElementHeight;
                VesselsDetails.RenderTransform = new TranslateTransform(-minX, -minY);
            }
        }

        /// <summary>
        /// Draws all parts of the KmlVessel that belongs to the given GuiVesselsNode.
        /// </summary>
        /// <param name="node">The GuiVesselsNode to read all parts from</param>
        public void DrawPartStructure(GuiVesselsNode node)
        {
            if (node != null)
            {
                DrawPartStructure(node.DataVessel);
            }
        }

        private void CalcMinMax(out double minX, out double minY, out double maxX, out double maxY)
        {
            double miX = 0;
            double miY = 0;
            double maX = 0;
            double maY = 0;
            foreach (UIElement element in VesselsDetails.Children)
            {
                double x = Canvas.GetLeft(element);
                double y = Canvas.GetTop(element);
                if (x < miX) miX = x;
                if (x > maX) maX = x;
                if (y < miY) miY = y;
                if (y > maY) maY = y;
            }
            minX = miX;
            minY = miY;
            maxX = maX;
            maxY = maY;
        }

        private Point CalcPosition(int x, int y)
        {
            Point p = new Point(x * ElementWidth, y * ElementHeight);
            return p;
        }

        private Point CalcPosition(IntPair koords)
        {
            return CalcPosition(koords.X, koords.Y);
        }

        private IntPair CalcKoords(IntPair parentKoords, int wantedX, int wantedY)
        {
            int diffX = wantedX - parentKoords.X;
            int diffY = wantedY - parentKoords.Y;
            // Check if it is free
            if (InsertGrid(wantedX, wantedY, null))
            {
                IntPair p = new IntPair(wantedX, wantedY);
                return p;
            }
            else
            {
                IntPair p;
                int i = 1;
                while (true)
                {
                    // always move away from root [0, 0]
                    int dx = i;
                    int dy = i;
                    if (wantedX < 0)
                        dx = -dx;
                    if (wantedY < 0)
                        dy = -dy;

                    if (diffY != 0)
                    {
                        p = new IntPair(wantedX + dx, wantedY);
                        if (InsertGrid(p.X, p.Y, null)) return p;
                        //p = new IntPair(wantedX - i, wantedY);
                        //if (InsertGrid(p.X, p.Y, null)) return p;
                    }
                    if (diffX != 0)
                    {
                        p = new IntPair(wantedX, wantedY + dy);
                        if (InsertGrid(p.X, p.Y, null)) return p;
                        //p = new IntPair(wantedX, wantedY - i);
                        //if (InsertGrid(p.X, p.Y, null)) return p;
                    }
                    p = new IntPair(wantedX + dx, wantedY + dy);
                    if (InsertGrid(p.X, p.Y, null)) return p;
                    //p = new IntPair(wantedX + i, wantedY - i);
                    //if (InsertGrid(p.X, p.Y, null)) return p;
                    //p = new IntPair(wantedX - i, wantedY + i);
                    //if (InsertGrid(p.X, p.Y, null)) return p;
                    //p = new IntPair(wantedX - i, wantedY - i);
                    //if (InsertGrid(p.X, p.Y, null)) return p;
                    i++;
                }
            }
        }

        private int CountTop(KmlPart part, KmlPart cameFrom)
        {
            if (part == cameFrom)
                return CountTop(part, new List<KmlPart>(), 0);
            else
                return CountTop(part, new List<KmlPart>() { cameFrom }, 0);
        }

        private int CountTop(KmlPart part, List<KmlPart> cameFrom, int overflow)
        {
            // A case was reported where this lead to a stack overflow.
            // Overflow exceptions are only caught in debug env, normal program just gets terminated.
            // To prevent this, stop calculation at some recursion depth.
            if (overflow > 1000) throw new OverflowException("Recursion too deep");

            // Now having this technical termination, try to avoid it by logic
            if (cameFrom.Contains(part))
                return 0;
            cameFrom.Add(part);

            int c = 1;
            if (part.AttachedPartsSurface.Count > 0)
            {
                c = 2;
                foreach (KmlPart p in part.AttachedPartsSurface)
                {
                    c = Math.Max(c, CountTop(p, cameFrom, overflow + 1) + 1);
                }
            }
            if (part.AttachedPartsBack.Count > 0)
            {
                c = Math.Max(c, 2);
                foreach (KmlPart p in part.AttachedPartsBack)
                {
                    c = Math.Max(c, CountTop(p, cameFrom, overflow + 1) + 1);
                }
            }
            foreach (KmlPart p in part.AttachedPartsFront)
            {
                c = Math.Max(c, CountTop(p, cameFrom, overflow + 1) - 1);
            }
            foreach (KmlPart p in part.AttachedPartsLeft)
            {
                c = Math.Max(c, CountTop(p, cameFrom, overflow + 1));
            }
            foreach (KmlPart p in part.AttachedPartsRight)
            {
                c = Math.Max(c, CountTop(p, cameFrom, overflow + 1));
            }
            if (part is KmlPartDock)
            {
                KmlPart p = ((KmlPartDock)part).DockedPart;
                if (p != null) c = Math.Max(c, CountTop(p, cameFrom, overflow + 1));
            }
            return c;
        }

        private int CountBottom(KmlPart part, KmlPart cameFrom)
        {
            if (part == cameFrom)
                return CountBottom(part, new List<KmlPart>(), 0);
            else
                return CountBottom(part, new List<KmlPart>() { cameFrom }, 0);
        }

        private int CountBottom(KmlPart part, List<KmlPart> cameFrom, int overflow)
        {
            // A case was reported where this lead to a stack overflow.
            // Overflow exceptions are only caught in debug env, normal program just gets terminated.
            // To prevent this, stop calculation at some recursion depth.
            if (overflow > 1000) throw new OverflowException("Recursion too deep");

            // Now having this technical termination, try to avoid it by logic
            if (cameFrom.Contains(part))
                return 0;
            cameFrom.Add(part);

            int c = 1;
            if (part.AttachedPartsSurface.Count > 2)
            {
                c = 2;
                foreach (KmlPart p in part.AttachedPartsSurface)
                {
                    c = Math.Max(c, CountBottom(p, cameFrom, overflow + 1) + 1);
                }
            }
            if (part.AttachedPartsFront.Count > 0)
            {
                c = Math.Max(c, 2);
                foreach (KmlPart p in part.AttachedPartsFront)
                {
                    c = Math.Max(c, CountBottom(p, cameFrom, overflow + 1) + 1);
                }
            }
            foreach (KmlPart p in part.AttachedPartsBack)
            {
                c = Math.Max(c, CountBottom(p, cameFrom, overflow + 1) - 1);
            }
            foreach (KmlPart p in part.AttachedPartsLeft)
            {
                c = Math.Max(c, CountBottom(p, cameFrom, overflow + 1));
            }
            foreach (KmlPart p in part.AttachedPartsRight)
            {
                c = Math.Max(c, CountBottom(p, cameFrom, overflow + 1));
            }
            if (part is KmlPartDock)
            {
                KmlPart p = ((KmlPartDock)part).DockedPart;
                if (p != null) c = Math.Max(c, CountBottom(p, cameFrom, overflow + 1));
            }
            return c;
        }

        private void InitGrid()
        {
            PartGrid = new List<List<GuiVesselsPartGraphNode>>();
            PartGrid.Add(new List<GuiVesselsPartGraphNode>());
            PartGrid[0].Add(null);
            PartGridMinX = 0;
            PartGridMinY = 0;
            PartGridMaxX = 0;
            PartGridMaxY = 0;
        }

        private bool InsertGrid(int x, int y, GuiVesselsPartGraphNode node)
        {
            // Grid koords [-4][-6] will be translated in list index [-4 - minX][-6 - minY]
            // so index will always be >= 0
            // Also keep the grid (list of lists) quadratic
            // On new min value create a new list, insert amount of blanks, append old list
            // and replace list by new list
            if (x < PartGridMinX)
            {
                int diffX = PartGridMinX - x;
                List<List<GuiVesselsPartGraphNode>> list = new List<List<GuiVesselsPartGraphNode>>();
                for (int i = 0; i < diffX; i++)
                {
                    List<GuiVesselsPartGraphNode> l = new List<GuiVesselsPartGraphNode>();
                    for (int j = PartGridMinY; j <= PartGridMaxY; j++)
                    {
                        l.Add(null);
                    }
                    list.Add(l);
                }
                list.AddRange(PartGrid);
                PartGrid = list;
                PartGridMinX = x;
            }
            if (y < PartGridMinY)
            {
                int diffY = PartGridMinY - y;
                for (int i = 0; i < PartGrid.Count ; i++)
                {
                    List<GuiVesselsPartGraphNode> list = new List<GuiVesselsPartGraphNode>();
                    for (int j = 0; j < diffY; j++)
                    {
                        list.Add(null);
                    }
                    list.AddRange(PartGrid[i]);
                    PartGrid[i] = list;
                }
                PartGridMinY = y;
            }
            if (x > PartGridMaxX)
            {
                int diffX = x - PartGridMaxX;
                for (int i = 0; i < diffX; i++)
                {
                    List<GuiVesselsPartGraphNode> list = new List<GuiVesselsPartGraphNode>();
                    for (int j = PartGridMinY; j <= PartGridMaxY; j++)
                    {
                        list.Add(null);
                    }
                    PartGrid.Add(list);
                }
                PartGridMaxX = x;
            }
            if (y > PartGridMaxY)
            {
                int diffY = y - PartGridMaxY;
                for (int i = 0; i < PartGrid.Count; i++)
                {
                    for (int j = 0; j < diffY; j++)
                    {
                        PartGrid[i].Add(null);
                    }
                }
                PartGridMaxY = y;
            }
            if (PartGrid[x - PartGridMinX][y - PartGridMinY] == null)
            {
                PartGrid[x - PartGridMinX][y - PartGridMinY] = node;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void DrawConnectedParts(GuiVesselsPartGraphNode parent, IntPair parentKoords)
        {
            KmlPart parentPart = parent.DataPart;
            IntPair newKoords;

            // Top atteched parts
            foreach (KmlPart sub in parentPart.AttachedPartsTop)
            {
                if (!sub.Visited)
                {
                    newKoords = CalcKoords(parentKoords, parentKoords.X, parentKoords.Y - CountBottom(sub, parentPart) - CountTop(parentPart, parentPart) + 1);
                    for (int y = parentKoords.Y - 1; y > newKoords.Y; y--)
                    {
                        // Fill space with dummy nodes
                        InsertGrid(parentKoords.X, y, new GuiVesselsPartGraphNode(parentKoords.X, y));
                    }
                    DrawPartAndConnected(sub, newKoords, parentKoords, ConnectVerticalBrush);
                }
                else
                {
                    DrawConnectionAndSeachPart(sub, parentKoords, ConnectVerticalBrush);
                }
            }

            // Bottom atteched parts
            foreach (KmlPart sub in parentPart.AttachedPartsBottom)
            {
                if (!sub.Visited)
                {
                    newKoords = CalcKoords(parentKoords, parentKoords.X, parentKoords.Y + CountTop(sub, parentPart) + CountBottom(parentPart, parentPart) - 1);
                    for (int y = parentKoords.Y + 1; y < newKoords.Y; y++)
                    {
                        // Fill space with dummy nodes
                        InsertGrid(parentKoords.X, y, new GuiVesselsPartGraphNode(parentKoords.X, y));
                    }
                    DrawPartAndConnected(sub, newKoords, parentKoords, ConnectVerticalBrush);
                }
                else
                {
                    DrawConnectionAndSeachPart(sub, parentKoords, ConnectVerticalBrush);
                }
            }

            // Left atteched parts
            foreach (KmlPart sub in parentPart.AttachedPartsLeft)
            {
                if (!sub.Visited)
                {
                    newKoords = CalcKoords(parentKoords, parentKoords.X - 2, parentKoords.Y);
                    DrawPartAndConnected(sub, newKoords, parentKoords, ConnectSidesBrush);
                }
                else
                {
                    DrawConnectionAndSeachPart(sub, parentKoords, ConnectSidesBrush);
                }
            }

            // Right atteched parts
            foreach (KmlPart sub in parentPart.AttachedPartsRight)
            {
                if (!sub.Visited)
                {
                    newKoords = CalcKoords(parentKoords, parentKoords.X + 2, parentKoords.Y);
                    DrawPartAndConnected(sub, newKoords, parentKoords, ConnectSidesBrush);
                }
                else
                {
                    DrawConnectionAndSeachPart(sub, parentKoords, ConnectSidesBrush);
                }
            }

            // Back atteched parts
            foreach (KmlPart sub in parentPart.AttachedPartsBack)
            {
                if (!sub.Visited)
                {
                    newKoords = CalcKoords(parentKoords, parentKoords.X - 2, parentKoords.Y - 1);
                    DrawPartAndConnected(sub, newKoords, parentKoords, ConnectSidesBrush);
                }
                else
                {
                    DrawConnectionAndSeachPart(sub, parentKoords, ConnectSidesBrush);
                }
            }

            // Front atteched parts
            foreach (KmlPart sub in parentPart.AttachedPartsFront)
            {
                if (!sub.Visited)
                {
                    newKoords = CalcKoords(parentKoords, parentKoords.X + 2, parentKoords.Y + 1);
                    DrawPartAndConnected(sub, newKoords, parentKoords, ConnectSidesBrush);
                }
                else
                {
                    DrawConnectionAndSeachPart(sub, parentKoords, ConnectSidesBrush);
                }
            }

            // Surface atteched parts
            int i = 0;
            foreach (KmlPart sub in parentPart.AttachedPartsSurface)
            {
                if (!sub.Visited)
                {
                    // Arrange alternating left and right
                    do
                    {
                        if (i % 4 == 0)
                        {
                            newKoords = new IntPair(parentKoords.X - (i / 4 + 1), parentKoords.Y - 1);
                        }
                        else if (i % 4 == 1)
                        {
                            newKoords = new IntPair(parentKoords.X + (i / 4 + 1), parentKoords.Y - 1);
                        }
                        else if (i % 4 == 2)
                        {
                            newKoords = new IntPair(parentKoords.X - (i / 4 + 1), parentKoords.Y + 1);
                        }
                        else
                        {
                            newKoords = new IntPair(parentKoords.X + (i / 4 + 1), parentKoords.Y + 1);
                        }
                        i++;
                    }
                    while (!InsertGrid(newKoords.X, newKoords.Y, null));
                    DrawPartAndConnected(sub, newKoords, parentKoords, ConnectSurfaceBrush);
                }
                else
                {
                    DrawConnectionAndSeachPart(sub, parentKoords, ConnectSurfaceBrush);
                    i++;
                }
            }

            // Docked parts
            if (parentPart is KmlPartDock)
            {
                KmlPartDock dock = (KmlPartDock)parentPart;
                KmlPart sub = dock.DockedPart;
                if (sub != null)
                {
                    if (!sub.Visited)
                    {
                        newKoords = CalcKoords(parentKoords, parentKoords.X + (sub.Position.X > parentPart.Position.X ? 1 : -1),
                            parentKoords.Y + (sub.Position.Y > parentPart.Position.Y ? 1 : -1));
                        DrawPartAndConnected(sub, newKoords, parentKoords, ConnectDockBrush);
                    }
                    else
                    {
                        DrawConnectionAndSeachPart(sub, parentKoords, ConnectDockBrush);
                    }
                }
            }
        }

        private void DrawPartAndConnected(KmlPart part, IntPair koords, IntPair parentKoords, Brush lineBrush)
        {
            GuiVesselsPartGraphNode partNode = DrawPart(part, koords);
            Line l = DrawConnection(parentKoords, koords, lineBrush);
            partNode.Lines.Add(l);
            GuiVesselsPartGraphNode parentNode = PartGrid[parentKoords.X - PartGridMinX][parentKoords.Y - PartGridMinY];
            parentNode.Lines.Add(l);
            DrawConnectedParts(partNode, koords);
        }

        private GuiVesselsPartGraphNode DrawPart(KmlPart part, int x, int y)
        {
            GuiVesselsPartGraphNode node = new GuiVesselsPartGraphNode(part, x, y);
            if(!InsertGrid(x, y, node))
            {
                throw new Exception("Couldn't place GuiVesselsPartGraphNode on grid pos " + x + ", " + y + " (already occupied)");
                //IntPair p = CalcKoords(new IntPair(x, y), x + 1, y);
                //InsertGrid(p.X, p.Y, node);
            }
            Point position = CalcPosition(x, y);
            Canvas.SetLeft(node, position.X);
            Canvas.SetTop(node, position.Y);
            // Have nodes on higher ZIndex than the lines
            Canvas.SetZIndex(node, 1);
            VesselsDetails.Children.Add(node);

            part.Visited = true;
            return node;
        }

        private GuiVesselsPartGraphNode DrawPart(KmlPart part, IntPair koords)
        {
            return DrawPart(part, koords.X, koords.Y);
        }

        private void DrawConnectionAndSeachPart(KmlPart part, IntPair parentKoords, Brush lineBrush)
        {
            // Already drawn sub part with one way arrow.
            // Need to draw the reverse arrow only
            // But where is the node?
            Point parentPos = CalcPosition(parentKoords);
            GuiVesselsPartGraphNode parentNode = PartGrid[parentKoords.X - PartGridMinX][parentKoords.Y - PartGridMinY];
            foreach (UIElement element in VesselsDetails.Children)
            {
                if (element is GuiVesselsPartGraphNode)
                {
                    GuiVesselsPartGraphNode node = (GuiVesselsPartGraphNode)element;
                    if (node.DataPart == part)
                    {
                        Line l = DrawConnection(parentPos.X, parentPos.Y, Canvas.GetLeft(node), Canvas.GetTop(node), lineBrush);
                        node.Lines.Add(l);
                        parentNode.Lines.Add(l);
                        break;
                    }
                }
            }
        }

        private Line DrawConnection(double x, double y, double parentX, double parentY, Brush lineBrush)
        {
            Line l = new Line();
            l.X1 = parentX + 12;
            l.Y1 = parentY + 8;
            l.X2 = x + 12;
            l.Y2 = y + 8;
            l.Stroke = lineBrush;
            VesselsDetails.Children.Add(l);
            return l;
        }

        private Line DrawConnection(IntPair koords, double parentX, double parentY, Brush lineBrush)
        {
            Point pos = CalcPosition(koords);
            return DrawConnection(pos.X, pos.Y, parentX, parentY, lineBrush);
        }

        private Line DrawConnection(double x, double y, IntPair parentKoords, Brush lineBrush)
        {
            Point parentPos = CalcPosition(parentKoords);
            return DrawConnection(x, y, parentPos.X, parentPos.Y, lineBrush);
        }

        private Line DrawConnection(IntPair koords, IntPair parentKoords, Brush lineBrush)
        {
            Point pos = CalcPosition(koords);
            Point parentPos = CalcPosition(parentKoords);
            return DrawConnection(pos.X, pos.Y, parentPos.X, parentPos.Y, lineBrush);
        }
    }
}

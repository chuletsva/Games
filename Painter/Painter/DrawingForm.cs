﻿using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Painter;
using Painter.Properties;

namespace Paint
{
    public partial class drawingForm : Form
    {
        private readonly ShapesPainter shapesPainter;
        private Image selectedImageForFilling;

        private ShapeType currentShapeType;
        private DrawingState drawingState;

        private Point selectedFirstPoint;
        private Point selectedSecondPoint;
        private Point cursorPoint;

        public drawingForm()
        {
            InitializeComponent();

            shapesPainter = new ShapesPainter();
            selectedImageForFilling = new Bitmap(patternsList.Images[0]);
            currentShapeType = ShapeType.Circle;
            drawingState = DrawingState.Waiting;

            cursorPoint = new Point(0, 0);
            selectedFirstPoint = new Point(0, 0);
            selectedSecondPoint = new Point(0, 0);

            BringToFontDrawingFormElements();
            AddPatternsInPatternsListView();
        }

        private void BringToFontDrawingFormElements()
        {
            patternsListView.BringToFront();
            menuStrip.BringToFront();
            toolStrip.BringToFront();
        }

        private void AddPatternsInPatternsListView()
        {
            foreach (var namePattern in patternsList.Images.Keys)
            {
                ListViewItem newItem = new ListViewItem
                {
                    Text = namePattern.Split('.').First(),
                    ImageKey = namePattern
                };

                patternsListView.Items.Add(newItem);
            }
        }

        // изменить currentShapeType
        private void drawCircleButton_Click(object sender, EventArgs e)
        {
            currentShapeType = ShapeType.Circle;
        }

        private void drawRectangleButton_Click(object sender, EventArgs e)
        {
            currentShapeType = ShapeType.Rectangle;
        }

        // рисовние фигруы на доске
        private void drawingPictureBox_Paint(object sender, PaintEventArgs e)
        {
            shapesPainter.DrawShapes(e.Graphics);

            if (drawingState == DrawingState.Drawing)
                e.Graphics.DrawLine(new Pen(Color.Black, 3), selectedFirstPoint, cursorPoint);
        }

        private void drawingPictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (drawingState == DrawingState.Drawing)
                return;

            drawingState = DrawingState.Drawing;
            selectedFirstPoint = cursorPoint;
        }

        private void drawingPictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (drawingState == DrawingState.Waiting)
                return;

            drawingState = DrawingState.Waiting;
            selectedSecondPoint = cursorPoint;
            AddCurrentShapeInFiguresPainter();
        }

        private void AddCurrentShapeInFiguresPainter()
        {
            TextureBrush textureBrush = new TextureBrush(selectedImageForFilling);
            Pen pen = new Pen(Color.Black, 3);

            int distanceBetweenFirstPointAndSecondPoint =
                (int) Math.Sqrt(Math.Pow(selectedFirstPoint.X - selectedSecondPoint.X, 2)
                + Math.Pow(selectedFirstPoint.Y - selectedSecondPoint.Y, 2));

            switch (currentShapeType)
            {
                case ShapeType.Circle:
                    shapesPainter.AddCircle(new Circle(selectedFirstPoint,
                        distanceBetweenFirstPointAndSecondPoint,
                        pen,
                        textureBrush));
                    break;

                case ShapeType.Rectangle:
                    shapesPainter.AddRectangle(new Rectangle(selectedFirstPoint,
                        2 * distanceBetweenFirstPointAndSecondPoint,
                        2 * distanceBetweenFirstPointAndSecondPoint,
                        pen,
                        textureBrush));
                    break;

                case ShapeType.Polygon:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(currentShapeType), currentShapeType, null);
            }
        }

        // Позиция курсора
        private void drawingPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            cursorPoint = new Point(e.X, e.Y);
            drawingPictureBox.Refresh();
        }

        // Очистить экран
        private void clearMenuItem_Click(object sender, EventArgs e)
        {
            ClearDrawingBoard();
        }

        // Отобразить тулбар
        private void toolbarMenuItem_Click(object sender, EventArgs e)
        {
            toolbarMenuItem.Checked = !toolbarMenuItem.Checked;
            toolStrip.Visible = toolbarMenuItem.Checked;
        }

        // Установить выбранный паттерн в качестве заливки
        private void patternsListView_ItemActivate(object sender, EventArgs e)
        {
            if (patternsListView.SelectedItems.Count == 0)
                return;

            selectedImageForFilling = patternsList.Images[patternsListView.SelectedIndices[0]];
        }

        // Сохранение файла
        private void saveAsFIleMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void saveFileButton_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void SaveFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Ptr format|*.ptr",
                FilterIndex = 0
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // TODO: чоделать
            }
        }

        // Отображение инфы разработчиков
        private void helpButton_Click(object sender, EventArgs e)
        {
            ShowHelpDialog();
        }

        private void aboutPainterMenuItem_Click(object sender, EventArgs e)
        {
            ShowHelpDialog();
        }

        private void ShowHelpDialog()
        {
            MessageBox.Show(Resources.InfoAboutProgram, "About program");
        }

        // Удалить последнюю нарисованную фигуру
        private void undoMenuItem_Click(object sender, EventArgs e)
        {
            EraseLastShape();
        }

        // Клавиши
        private void drawingForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z)
                EraseLastShape();

            if (e.KeyCode == Keys.Delete)
                ClearDrawingBoard();
        }

        private void EraseLastShape()
        {
            shapesPainter.DeleteLastShape();
            drawingPictureBox.Refresh();
        }

        private void ClearDrawingBoard()
        {
            shapesPainter.ClearFigures();
            drawingPictureBox.Refresh();
        }

        private void pagePropertyMenuItem_Click(object sender, EventArgs e)
        {
            new PagePropertyForm(drawingPictureBox).ShowDialog();
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
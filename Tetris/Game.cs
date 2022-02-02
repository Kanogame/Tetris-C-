﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tetris
{
    class Game : IDisposable
    {
        private Field field;
        // System.Windows.Forms.Timer
        private Timer timer;
        //timeout ms when moving down
        private Timer secondsTimer;
        private int normalSpeed;
        //timeout ms when quick moving down
        private int spacespeed;

        private int secondsPassed = 0;

        private AdditionalPanel additionalPanel;

        public event Action RepaintRequired;

        private int scores;
        private GamesState gamesState;
        private int levels;
        private int linesRemoved;
        private MainMenu mainMenu; 

        //private TetrisOneLove TetrisOneLove;

        public Game(TetrisOneLove killer)
        {
            this.mainMenu = new MainMenu();
            //this.TetrisOneLove = killer;
            gamesState = GamesState.Gaming;
            levels = 1;
            scores = 0;
            linesRemoved = 0;
            additionalPanel = new AdditionalPanel();
            additionalPanel.setLevel(levels);
            normalSpeed = 200;
            spacespeed = 30;
            field = new Field(rows: 18, columns: 9, padding: 50);
            field.NewFiguerCreated += Field_NewFiguerCreated;
            field.LinesRemoved += Field_LinesRemoved;
            field.start();
            timer = new Timer();
            timer.Interval = normalSpeed;
            timer.Tick += Timer_Tick;
            timer.Start();
            secondsTimer = new Timer();
            secondsTimer.Interval = 1000;
            secondsTimer.Tick += secondsTimer_Tick;
            secondsTimer.Start();
        }

        private void Field_LinesRemoved(int removedCount)
        {
            linesRemoved += removedCount;
            scores += removedCount * 100;
            additionalPanel.setLinesRemoved(linesRemoved);
            additionalPanel.setScores(scores);
            invokeRepaintRequired();
        }

        private void secondsTimer_Tick(object sender, EventArgs e)
        {
            secondsPassed++;
            additionalPanel.setSecondsPassed(secondsPassed);
        }

        private void Field_NewFiguerCreated(Figure current, Figure next)
        {
            additionalPanel.setFigure(next);
            if (timer != null)
            {
                timer.Interval = normalSpeed;
            }
        }

        private void invokeRepaintRequired()
        {
            if (RepaintRequired != null)
            {
                RepaintRequired();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if(!field.moveFigureDown())
            {
                timer.Stop();
                secondsTimer.Stop();
                if (MessageBox.Show("Игра окончена, научись играть", "вопрос", MessageBoxButtons.YesNoCancel) !=DialogResult.Yes)
                {
                    MessageBox.Show("Врать не хорошо");
                    //TetrisOneLove.kill();
                }
                gamesState = GamesState.GameOver;
            }
            invokeRepaintRequired();
        }

        public void Dispose()
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }

        public void display(Graphics g, Size containerSize)
        {
            var ClientRect = new Rectangle(0, 0, containerSize.Width, containerSize.Height);

            mainMenu.display(g, ClientRect);
            return;

            if (gamesState == GamesState.GameOver)
            {
                ImageDrawer.fit(g, ClientRect, Config.GameOverImage);
            }
                var FieldArea = field.GetRectangle(containerSize);
                field.display(g, containerSize);
                var additionalPanelArea = new Rectangle(FieldArea.Right, FieldArea.Top,
                    containerSize.Width - FieldArea.Right, FieldArea.Height);
                additionalPanel.display(g, additionalPanelArea, gamesState);
        }

        public void toLeft()
        {
            field.toLeft();
            invokeRepaintRequired();
        }

        public void toRight()
        {
            field.toRight();
            invokeRepaintRequired();
        }

        public void rotate()
        {
            field.rotate();
            invokeRepaintRequired();
        }

        public void quick()
        {
            timer.Interval = spacespeed;
        }
    }
}

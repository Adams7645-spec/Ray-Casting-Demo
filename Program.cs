using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;
using static System.Array;

namespace Ray_Casting
{
    internal class Program
    {
        private const int windowWidth = 120;
        private const int windowHight = 60;

        private const int mapWidth = 20;
        private const int mapHight = 20;

        private const double Fov = Math.PI / 3;
        private const double Depth = 10;

        private static double playerX = 1;
        private static double playerY = 1;
        private static double playerA = 0;

        private static string map = "";

        private static readonly char[] Window = new char[windowWidth * windowHight];
        static void Main(string[] args)
        {
            SetWindowSize(windowWidth, windowHight);
            SetBufferSize(windowWidth, windowHight);
            CursorVisible = false;

            map += "####################";
            map += "#........#.........#";
            map += "#..................#";
            map += "#........#.........#";
            map += "##########.........#";
            map += "#........#.........#";
            map += "#..................#";
            map += "#........#.........#";
            map += "#........####...####";
            map += "#........#.........#";
            map += "#........#.........#";
            map += "#........#.........#";
            map += "###..#######.......#";
            map += "#....#.....#.......#";
            map += "#..................#";
            map += "#....#...#####..####";
            map += "#....#.............#";
            map += "#..................#";
            map += "#....#.............#";
            map += "####################";

            DateTime dateTimeFrom = DateTime.Now;

            while (true)
            {
                DateTime dateTimeTo = DateTime.Now;
                double elepsedTime = (dateTimeTo - dateTimeFrom).TotalSeconds;
                dateTimeFrom = DateTime.Now;

                if (KeyAvailable)
                {
                    var consoleKey = ReadKey(true).Key;

                    switch (consoleKey)
                    {
                        case ConsoleKey.A:
                            playerA += elepsedTime * 10;
                            break;
                        case ConsoleKey.D:
                            playerA -= elepsedTime * 10;
                            break;
                        case ConsoleKey.W:
                            {
                                playerX += Math.Sin(playerA) * 15 * elepsedTime;
                                playerY += Math.Cos(playerA) * 15 * elepsedTime;

                                if (map[(int)playerY * mapWidth + (int)playerX] == '#')
                                {
                                    playerX -= Math.Sin(playerA) * 15 * elepsedTime;
                                    playerY -= Math.Cos(playerA) * 15 * elepsedTime;
                                }
                                break;
                            }
                        case ConsoleKey.S:
                            {
                                playerX -= Math.Sin(playerA) * 15 * elepsedTime;
                                playerY -= Math.Cos(playerA) * 15 * elepsedTime;

                                if (map[(int)playerY * mapWidth + (int)playerX] == '#')
                                {
                                    playerX += Math.Sin(playerA) * 15 * elepsedTime;
                                    playerY += Math.Cos(playerA) * 15 * elepsedTime;
                                }
                                break;
                            }
                    }
                }

                for (int X = 0; X < windowWidth; X++)
                {
                    double rayAngle = playerA + Fov / 2 - X * Fov / windowWidth;

                    double rayX = Math.Sin(rayAngle);
                    double rayY = Math.Cos(rayAngle);

                    double distanceToWall = 0;
                    bool hitWall = false;
                    bool isBound = false;

                    while (!hitWall && distanceToWall < Depth)
                    {
                        distanceToWall += 0.1;

                        int testX = (int)(playerX + rayX * distanceToWall);
                        int testY = (int)(playerY + rayY * distanceToWall);

                        if (testX < 0 || testX >= Depth + playerX || testY < 0 || testY >= Depth + playerY)
                        {
                            hitWall = true;
                            distanceToWall = Depth;
                        }
                        else
                        {
                            char testCell = map[testY * mapWidth + testX];
                            if (testCell == '#')
                            {
                                hitWall = true;

                                var boundsVectorList = new List<(double module, double cos)>();

                                for (int tx = 0; tx < 2; tx++)
                                {
                                    for (int ty = 0; ty < 2; ty++)
                                    {
                                        double vx = testX + tx - playerX;
                                        double vy = testY + ty - playerY;

                                        double vectorModule = Math.Sqrt(vx * vx + vy * vy);
                                        double cosAngle = rayX * vx / vectorModule + rayY * vy / vectorModule;

                                        boundsVectorList.Add((vectorModule, cosAngle));
                                    }
                                }
                                boundsVectorList = boundsVectorList.OrderBy(v => v.module).ToList();

                                double boundAngle = 0.03 / distanceToWall;

                                if (Math.Acos(boundsVectorList[0].cos) < boundAngle ||
                                    Math.Acos(boundsVectorList[1].cos) < boundAngle)
                                {
                                    isBound = true;
                                }
                            }
                        }
                    }

                    int ceiling = (int)(windowHight / 2d - windowHight * Fov / distanceToWall);
                    int floor = windowHight - ceiling;

                    char wallShade;

                    if (isBound)
                    {
                        wallShade = '|';
                    }
                    else if (distanceToWall <= Depth / 4d)
                    {
                        wallShade = '\u2588';
                    }
                    else if (distanceToWall < Depth / 3d)
                    {
                        wallShade = '\u2593';
                    }
                    else if (distanceToWall < Depth / 2d)
                    {
                        wallShade = '\u2592';
                    }
                    else if (distanceToWall < Depth)
                    {
                        wallShade = '\u2591';
                    }
                    else
                    {
                        wallShade = ' ';
                    }

                    for (int Y = 0; Y < windowHight; Y++)
                    {
                        if (Y <= ceiling)
                        {
                            Window[Y * windowWidth + X] = ' ';
                        }
                        else if (Y > ceiling && Y <= floor)
                        {
                            Window[Y * windowWidth + X] = wallShade;
                        }
                        else
                        {
                            char floorShade;

                            double b = 1 - (Y - windowHight / 2d) / (windowHight / 2d);
                            if (b < 0.25)
                            {
                                floorShade = '#';
                            }
                            else if (b < 0.5)
                            {
                                floorShade = 'x';
                            }
                            else if (b < 0.75)
                            {
                                floorShade = '-';
                            }
                            else if (b < 0.9)
                            {
                                floorShade = '.';
                            }
                            else
                            {
                                floorShade = ' ';
                            }

                            Window[Y * windowWidth + X] = floorShade;
                        }
                    }
                }

                char[] stats = $"X: {playerX}, Y: {playerY}, A: {playerA}, FPS: {(int)(1 / elepsedTime)}".ToCharArray();
                stats.CopyTo(Window, 0);

                SetCursorPosition(0, 0);
                Write(Window);
            }
        }
    }
}

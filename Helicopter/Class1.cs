
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using VRageMath;
using VRage.Game;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Ingame;
using Sandbox.Game.EntityComponents;
using VRage.Game.Components;
using VRage.Collections;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
public sealed class Program : MyGridProgram
{
    // НАЧАЛО СКРИПТА

    String shipName = "Блу";

    const double gyroAccuracy = 0.002;
    const double gyroMinSpeed = 0.1;

    // IMyCameraBlock forwardCamera;
    List<IMyThrust> upperTrusters = new List<IMyThrust>();
    IMyShipController shipController;
    IMyGyro gyro;
    // IMyCockpit cocpit = null;
    IMyTextSurface screen = null;

    public Program()
    {
        Runtime.UpdateFrequency = UpdateFrequency.Update1;

        screen = GridTerminalSystem.GetBlockWithName("Дисплей " + shipName) as IMyTextSurface;
        if (screen != null)
        {
            screen.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            screen.FontSize = 1f;
        }
        else
        {
            Echo("Экран не найден");
        }

        shipController = GridTerminalSystem.GetBlockWithName("Управление " + shipName) as IMyShipController;
        // shipController = cocpit;
        if (shipController != null)
        {
        }
        else
        {
            Echo("Управление не найдено");
        }

        gyro = GridTerminalSystem.GetBlockWithName("Гироскоп " + shipName) as IMyGyro;
        if (gyro != null)
        {
            // gyro.GyroOverride = true;
        }
        else
        {
            Echo("Гироскоп не найдено");
        }

        /*forwardCamera = GridTerminalSystem.GetBlockWithName("Камера Вперёд " + shipName) as IMyCameraBlock;
        if (forwardCamera != null)
        {
            // gyro.GyroOverride = true;
        }
        else
        {
            Echo("Камера вперёд не найден");
        }*/

        var thrusters = new List<IMyThrust>();
        GridTerminalSystem.GetBlockGroupWithName("Двигатели Вверх " + shipName).GetBlocksOfType<IMyThrust>(upperTrusters);
        GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters);/*
        Echo("Количество двигателей: " + thrusters.Count.ToString());
        for (int i = 0; i < thrusters.Count; i++)
        {
            if (thrusters[i].GridThrustDirection.Y == -1)
            {
                upperTrusters.Add(thrusters[i]);
            }
        }*/

        if (upperTrusters.Count != 0)
        {
        }
        else
        {
            Echo("Двигатели вверх не найдены");
        }
    }

    public void Main(string args)
    {
        StringBuilder message = new StringBuilder();
        message.Append("Статус: ");
        if (isPlayerControl(shipController))
        {
            message.Append("Корабль под управлением\n");
            gyro.GyroOverride = false;
        }
        else
        {
            message.Append("Корабль без управления\n");

            gyro.Yaw = 0;
            gyro.Pitch = 0;
            gyro.Roll = 0;

            // Vector3D shipVectorRight = forwardCamera.WorldMatrix.Right;
            // Vector3D shipVectorDown = forwardCamera.WorldMatrix.Down;
            // Vector3D shipVectorForward = forwardCamera.WorldMatrix.Forward;
            Vector3D shipVectorRight = shipController.WorldMatrix.Right;
            Vector3D shipVectorDown = shipController.WorldMatrix.Down;
            Vector3D shipVectorForward = shipController.WorldMatrix.Forward;
            Vector3D gravity = shipController.GetTotalGravity();
            gravity.Normalize();

            double complanarRoll = complanarTest(shipVectorDown, shipVectorForward, gravity);
            double complanarPitch = complanarTest(shipVectorDown, shipVectorRight, gravity);

            var startAccelerate = 0.3;
            var coefAccelerate = 3;
            if (complanarRoll > startAccelerate)
            {
                complanarRoll *= coefAccelerate;
            }

            if (complanarPitch > startAccelerate)
            {
                complanarPitch *= coefAccelerate;
            }

            if (Math.Abs(complanarRoll) < gyroAccuracy)
            {
                gyro.Roll = 0;
                // message.Append("Корабль выровнен по вращению\n");
            }
            else
            {
                gyro.GyroOverride = true;
                gyro.Roll = (float)maxAbs(complanarRoll, gyroMinSpeed);
            }
            if (Math.Abs(complanarPitch) < gyroAccuracy)
            {
                gyro.Pitch = 0;
                // message.Append("Корабль выровнен по тангажу\n");
            }
            else
            {
                gyro.GyroOverride = true;
                gyro.Pitch = (float)maxAbs(-complanarPitch, gyroMinSpeed);
            }

            // message.Append("Коэффициент вращения: " + complanarRoll.ToString() + "\n");
            // message.Append("Коэффициент тангажа: " + complanarPitch.ToString() + "\n");

            for (int i = 0; i < upperTrusters.Count; i++)
            {
                var truster = upperTrusters[i];
            }
        }
        message.Append("Рыскание: " + gyro.Yaw.ToString() + "\n");
        message.Append("Количество трастеров вверх: " + upperTrusters.Count.ToString() + "\n");
        message.Append("Текущее ускорение: " + upperTrusters[0].CurrentThrust.ToString() + "\n");
        message.Append("Макс ускорение: " + upperTrusters[0].MaxThrust.ToString() + "\n");
        message.Append("Макс эфф. ускорение: " + upperTrusters[0].MaxEffectiveThrust.ToString() + "\n");
        message.Append("Сила гравитации: " + shipController.GetTotalGravity().Length().ToString() + "\n");
        message.Append("Масса: " + shipController.CalculateShipMass().TotalMass.ToString() + "\n");
        // F = ma a = mF
        var calcThrust = shipController.GetTotalGravity().Length() * shipController.CalculateShipMass().TotalMass / upperTrusters.Count;
        message.Append("Расчитанное ускорение: " + calcThrust.ToString() + "\n");

        screen.WriteText(message);
    }

    public void Save()
    {
    }

    public double complanarTest(Vector3D vector1, Vector3D vector2, Vector3D vector3)
    {
        /*Matrix3x3 matrix = new Matrix3x3();
        matrix.SetRow(0, vector1);
        matrix.SetRow(1, vector1);
        matrix.SetRow(2, vector1);
        return matrix.Determinant();*/

        double m11 = vector1.X;
        double m12 = vector1.Y;
        double m13 = vector1.Z;
        double m21 = vector2.X;
        double m22 = vector2.Y;
        double m23 = vector2.Z;
        double m31 = vector3.X;
        double m32 = vector3.Y;
        double m33 = vector3.Z;

        // m11 m12 m13
        // m21 m22 m23
        // m31 m32 m33

        double determinant = m11 * m22 * m33 + m12 * m23 * m31 + m13 * m21 * m32 - m13 * m22 * m31 - m12 * m21 * m33 - m11 * m23 * m32;

        return determinant;
    }

    public bool isPlayerControl(IMyShipController shipController)
    {
        double roll = shipController.RollIndicator;
        Vector3 move = shipController.MoveIndicator;
        Vector2 rotation = shipController.RotationIndicator;

        return (roll != 0) || (move.X != 0) || (move.Y != 0) || (move.Z != 0) || (rotation.X != 0) || (rotation.Y != 0);
    }

    public double maxCoordinateDiffrence(Vector3D vector1, Vector3D vector2)
    {
        var deltaX = Math.Abs(vector1.X - vector2.X);
        var deltaY = Math.Abs(vector1.Y - vector2.Y);
        var deltaZ = Math.Abs(vector1.Z - vector2.Z);

        return Math.Max(deltaX, Math.Max(deltaY, deltaZ));
    }

    public double maxAbs(double target, double limit)
    {
        if (target < 0)
        {
            return (target < -limit) ? target : -limit;
        }
        else
        {
            return (target > limit) ? target : limit;
        }
    }
    // КОНЕЦ СКРИПТА


    // Vector3I lowerTrusterVector = lowerTruster.;
    // Vector3 moveVector = shipController.MoveIndicator;
    // Vector2 rotationVector = shipController.RotationIndicator;
    // float roll = shipController.RollIndicator;
    // gyro.Yaw = rotationVector.Y;

    // stringBuilder.Append("X = " + rotationVector.X.ToString() + "\n");
    // stringBuilder.Append("Y = " + rotationVector.Y.ToString() + "\n");
    // stringBuilder.Append("roll = " + roll.ToString() + "\n");

    /*stringBuilder.Append("Vector Coef: \n");
    stringBuilder.Append("X = " + (gravity.X / shipVector.X).ToString() + "\n");
    stringBuilder.Append("Y = " + (gravity.Y / shipVector.Y).ToString() + "\n");
    stringBuilder.Append("Z = " + (gravity.Y / shipVector.Y).ToString() + "\n");

    stringBuilder.Append("Gravity Vector: \n");
    stringBuilder.Append("X = " + gravity.X.ToString() + "\n");
    stringBuilder.Append("Y = " + gravity.Y.ToString() + "\n");
    stringBuilder.Append("Z = " + gravity.Z.ToString() + "\n");

    stringBuilder.Append("Ship Vector: \n");
    stringBuilder.Append("X = " + shipVector.X.ToString() + "\n");
    stringBuilder.Append("Y = " + shipVector.Y.ToString() + "\n");
    stringBuilder.Append("Z = " + shipVector.Z.ToString() + "\n");

    if (moveVector.Z != 0)
    {
        if (moveVector.Z == -1)
        {
            stringBuilder.Append("W|");
        }
        else
        {
            stringBuilder.Append("S|");
        }
    }
    if (moveVector.X != 0)
    {
        if (moveVector.X == -1)
        {
            stringBuilder.Append("A|");
        }
        else
        {
            stringBuilder.Append("D|");
        }
    }
    if (moveVector.Y != 0)
    {
        if (moveVector.Y == -1)
        {
            stringBuilder.Append("C|");
        }
        else
        {
            stringBuilder.Append("Space|");
        }
    }
    stringBuilder.Append("\n");
    if (rotationVector.X != 0)
    {
        if (rotationVector.X < 0)
        {
            stringBuilder.Append("Вниз + " + rotationVector.X + "\n");
        }
        else
        {
            stringBuilder.Append("Вверх + " + rotationVector.X + "\n");
        }
    }
    if (rotationVector.Y != 0)
    {
        if (rotationVector.Y < 0)
        {
            stringBuilder.Append("Влево + " + rotationVector.Y + "\n");
        }
        else
        {
            stringBuilder.Append("Вправо + " + rotationVector.Y + "\n");
        }
    }
    if (roll != 0)
    {
        if (roll < 0)
        {
            stringBuilder.Append("Вращение против часовой");
        }
        else
        {
            stringBuilder.Append("Вращение по часовой");
        }
    }*/

    // rotor.LowerLimitDeg = i;
    // rotor.UpperLimitDeg = i;
    // rotor.TargetVelocityRPM = 100;
    // i += 90;
    // if (i == 360)
    // {
    //     i = 0;
    // }

    // screen.WriteText(i.ToString());
    // i++;


    // Конструктор, вызванный единожды в каждой сессии и
    //  всегда перед вызовом других методов. Используйте его,
    // чтобы инициализировать ваш скрипт.
    //  
    // Конструктор опционален и может быть удалён, 
    // если в нём нет необходимости.
    // 
    // Рекомендуется использовать его, чтобы установить RuntimeInfo.UpdateFrequency
    // , что позволит перезапускать ваш скрипт
    // автоматически, без нужды в таймере.

    // Главная точка входа в скрипт вызывается каждый раз,
    // когда действие Запуск программного блока активируется,
    // или скрипт самозапускается. Аргумент updateSource описывает,
    // откуда поступило обновление.
    // 
    // Метод необходим сам по себе, но аргументы
    // ниже могут быть удалены, если не требуются.

    // Вызывается, когда программе требуется сохранить своё состояние.
    // Используйте этот метод, чтобы сохранить состояние программы в поле Storage,
    // или в другое место.
    // 
    // Этот метод опционален и может быть удалён,
    // если не требуется.
}
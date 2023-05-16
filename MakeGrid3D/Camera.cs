using System;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Media.Media3D;

namespace MakeGrid3D
{
    public class Camera
    {
        private Vector3 front = -Vector3.UnitZ;
        private Vector3 up = Vector3.UnitY;
        private Vector3 right = Vector3.UnitX;
        private Vector3 defaultPosition;
        // in radians
        private float pitch;
        private float yaw = -MathHelper.PiOver2; // Without this, you would be started rotated 90 degrees right.
        private float fov = MathHelper.PiOver2;

        public float Speed { get; set; } = Default.speedMove;
        public Vector3 Position { get; set; }
        public float AspectRatio { private get; set; }
        public Vector3 Front => front;
        public Vector3 Up => up;
        public Vector3 Right => right;
        // We convert from degrees to radians as soon as the property is set to improve performance.
        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(pitch);
            set
            {
                // We clamp the pitch value between -89 and 89 to prevent the camera from going upside down, and a bunch
                // of weird "bugs" when you are using euler angles for rotation.
                // If you want to read more about this you can try researching a topic called gimbal lock
                var angle = MathHelper.Clamp(value, -89f, 89f);
                pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        // We convert from degrees to radians as soon as the property is set to improve performance.
        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(yaw);
            set
            {
                yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        // The field of view (FOV) is the vertical angle of the camera view.
        // we can use this to simulate a zoom feature.
        // We convert from degrees to radians as soon as the property is set to improve performance.
        public float Fov
        {
            get => MathHelper.RadiansToDegrees(fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 90f);
                fov = MathHelper.DegreesToRadians(angle);
            }
        }

        public Camera() { }
        public Camera(Vector3 position, float aspectRatio)
        {
            defaultPosition = position;
            Position = defaultPosition;
            AspectRatio = aspectRatio;
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + front, up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(fov, AspectRatio, 0.01f, 1000f);
        }

        private void UpdateVectors()
        {
            front.X = MathF.Cos(pitch) * MathF.Cos(yaw);
            front.Y = MathF.Sin(pitch);
            front.Z = MathF.Cos(pitch) * MathF.Sin(yaw);

            // We need to make sure the vectors are all normalized, as otherwise we would get some funky results.
            front = Vector3.Normalize(front);

            // Calculate both the right and the up vector using cross product.
            // Note that we are calculating the right from the global up; this behaviour might
            // not be what you need for all cameras so keep this in mind if you do not want a FPS camera.
            right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
            up = Vector3.Normalize(Vector3.Cross(right, front));
        }

        public void Reset()
        {
            Position = defaultPosition;
            front = -Vector3.UnitZ;
            up = Vector3.UnitY;
            right = Vector3.UnitX;
            yaw = -MathHelper.PiOver2;
            fov = MathHelper.PiOver2;
            Speed = Default.speedMove;
        }

        // TODO: Нужно добавить умножение на время, иначе чем мощнее компьютер чем быстрее будет камера
        public void MoveForward()
        {
            Position += front * Speed;
        }
        public void MoveBackwards()
        {
            Position -= front * Speed;
        }
        public void MoveRight()
        {
            Position += right * Speed;
        }
        public void MoveLeft()
        {
            Position -= right * Speed;
        }
        public void MoveUp()
        {
            Position += up * Speed;
        }
        public void MoveDown()
        {
            Position -= up * Speed;
        }
        public void Zoom(float delta)
        {
            Fov -= delta;
        }
    }
}

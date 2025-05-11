global using Vector2D = OpenTK.Mathematics.Vector2;
global using Vector3D = OpenTK.Mathematics.Vector3;
using System;
using OpenTK.Mathematics;
namespace MakeGrid3D
{
    public class Camera
    {
        private Vector3D front = -Vector3D.UnitZ;
        private Vector3D up = Vector3D.UnitY;
        private Vector3D right = Vector3D.UnitX;
        private Vector3D defaultPosition;
        // in radians
        private float pitch;
        private float yaw = -MathHelper.PiOver2; // Without this, you would be started rotated 90 degrees right.
        private float fov = MathHelper.PiOver2;

        public float Speed { get; set; } = Default.speedMove;
        public Vector3D Position { get; set; }
        public float AspectRatio { private get; set; }
        public Vector3D Front => front;
        public Vector3D Up => up;
        public Vector3D Right => right;
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
        public Camera(Vector3D position, float aspectRatio)
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
            front = Vector3D.Normalize(front);

            // Calculate both the right and the up vector using cross product.
            // Note that we are calculating the right from the global up; this behaviour might
            // not be what you need for all cameras so keep this in mind if you do not want a FPS camera.
            right = Vector3D.Normalize(Vector3D.Cross(front, Vector3D.UnitY));
            up = Vector3D.Normalize(Vector3D.Cross(right, front));
        }

        public void Reset()
        {
            Position = defaultPosition;
            front = -Vector3D.UnitZ;
            up = Vector3D.UnitY;
            right = Vector3D.UnitX;
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Engine : MonoBehaviour {

    private struct Particle
    {
        public Vector3 position;
        public Vector3 velocity;
    }

    static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
    {
        if (length == 1) return list.Select(t => (IEnumerable<T>)new T[] { t });

        return GetPermutations(list, length - 1)
            .SelectMany((t) => list.Where(e => !t.Contains(e)),
                (t1, t2) => t1.Concat(new T[] { t2 }));
    }

    // Public Variables
    public int particleCount = 100000;
    public ComputeShader computeShader;
    public float Acceleration = 0.01f;
    public float Damping = 0.01f;
    public Material dustMaterial;
    public GameObject gravityObject;

    private const int PARTICLE_SIZE = sizeof(float) * 6;
    private ComputeBuffer particleBuffer;
    private ComputeBuffer resetBuffer;
    private int updateKernelId;
    private int resetKernelId;
    private int threadGroups;
    private Color[] colors;
    private List<IEnumerable<Color>> options;
    private bool paused = true;
    private float pausedTime = 0;


    // Use this for initialization
    void Start() {
        // Setup Colors
        colors = new Color[8] {
            rgbColor(11, 2, 62), // Dark Blue
            rgbColor(145, 0, 0), // Dark Red
            rgbColor(99, 40, 95), // Dark Purple
            rgbColor(38, 101, 27), // Dark Green

            rgbColor(69, 108, 255), // Light Blue
            rgbColor(255, 158, 0), // Orange
            rgbColor(94, 255, 0), // Light Green
            rgbColor(255, 248, 0) // Yellow
        };
        options = new List<IEnumerable<Color>>(GetPermutations<Color>(colors, 2));
        // Calculate useful variables
        updateKernelId = computeShader.FindKernel("CSMain");
        resetKernelId = computeShader.FindKernel("Reset");
        threadGroups = Mathf.CeilToInt((float)particleCount / 256.0f);

        // Initialize the particle data
        Particle[] particles = new Particle[particleCount];

        for (int i = 0; i < particleCount; i++)
        {
            // setup particles with random position and velocity of 0
            // Initial Position
            particles[i] = new Particle();
            particles[i].position.x = Random.Range(-0.5f, 0.5f);
            particles[i].position.y = Random.Range(0.5f, 1.5f);
            particles[i].position.z = Random.Range(-0.5f, 0.5f);

            // Initial Velocity
            particles[i].velocity = Vector3.zero;
        }

        particleBuffer = new ComputeBuffer(particleCount, PARTICLE_SIZE);
        particleBuffer.SetData(particles);
        resetBuffer = new ComputeBuffer(particleCount, PARTICLE_SIZE);
        resetBuffer.SetData(particles);

        computeShader.SetBuffer(updateKernelId, "particleBuffer", particleBuffer);

        computeShader.SetBuffer(resetKernelId, "particleBuffer", particleBuffer);
        computeShader.SetBuffer(resetKernelId, "resetBuffer", resetBuffer);
        dustMaterial.SetBuffer("particleBuffer", particleBuffer);
    }

    // Update is called once per frame
    void Update() {
        Vector3 pos = gravityObject.transform.position;
        computeShader.SetVector("gravityObjectPosition", pos);
        computeShader.SetFloat("acceleration", Acceleration);
        computeShader.SetFloat("damping", Damping);
        float lerpTime = 0.3f;
        if (paused)
        {
            computeShader.SetFloat("deltaTime", Mathf.Lerp(Time.deltaTime, 0.0f, (Time.fixedTime - pausedTime)/lerpTime));
        }
        else
        {
            computeShader.SetFloat("deltaTime", Mathf.Lerp(0.0f, Time.deltaTime, (Time.fixedTime - pausedTime) / lerpTime));
        }
        computeShader.Dispatch(updateKernelId, threadGroups, 1, 1);
    }

    public void Reset()
    {
        computeShader.Dispatch(resetKernelId, threadGroups, 1, 1);
    }

    public void SetAcceleration(float acceleration)
    {
        this.Acceleration = acceleration;
    }

    public void SetDamping(float damping)
    {
        this.Damping = damping;
    }

    public void SetSpeedColorThreshold(float thresh)
    {
        dustMaterial.SetFloat("_FastThreshold", thresh);
    }

    public void setColorIndex(int index)
    {

        List<Color> selection = new List<Color>(options[Mathf.Abs(index) % options.Count]);

        dustMaterial.SetColor("_SlowColor", selection[0]);
        dustMaterial.SetColor("_FastColor", selection[1]);
    }

    public void TogglePause()
    {
        paused = !paused;
        pausedTime = Time.fixedTime;
    }

    // Called after Update
    void OnRenderObject()
    {
        dustMaterial.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Points, 1, particleCount);
    }

    void OnDestroy()
    {
        if (particleBuffer != null)
        {
            particleBuffer.Dispose();
        }
        if (resetBuffer != null)
        {
            resetBuffer.Dispose();
        }
    }

    private Color rgbColor(int r, int g, int b)
    {
        return new Color((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f);
    }
}

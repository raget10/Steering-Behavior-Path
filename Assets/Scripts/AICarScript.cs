using UnityEngine;

public class AICarScript : MonoBehaviour
{
    // Titik pusat gravitasi kendaraan
    public Vector3 centerOfMass;
    // Array untuk menyimpan titik jalur
    private Transform[] path;
    // Objek grup jalur yang akan diambil anak-anaknya sebagai titik jalur
    public Transform pathGroup;
    // Maksimal sudut kemudi
    public float maxSteer = 15.0f;
    // Collider roda depan dan belakang
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;
    // Indeks titik jalur saat ini
    public int currentPathObj;
    // Jarak minimal ke titik jalur untuk pindah ke titik berikutnya
    public float distFromPath = 20f;
    // Torsi maksimal yang diterapkan pada roda belakang
    public float maxTorque = 50f;
    // Kecepatan mobil saat ini
    public float currentSpeed;
    // Kecepatan tertinggi mobil
    public float topSpeed = 150f;
    // Kecepatan deselerasi (pengurangan kecepatan)
    public float decelerationSpeed = 10f;
    // Komponen Rigidbody mobil
    private Rigidbody rigidBody;

    // Fungsi Start dipanggil sekali saat objek diinisialisasi
    private void Start()
    {
        // Dapatkan komponen Rigidbody dari mobil
        rigidBody = GetComponent<Rigidbody>();
        // Set pusat gravitasi
        rigidBody.centerOfMass = centerOfMass;
        // Ambil jalur
        GetPath();
    }

    // Fungsi Update dipanggil sekali setiap frame
    private void Update()
    {
        // Kontrol kemudi dan gerakan
        GetSteer();
        Move();
    }

    // Fungsi untuk mendapatkan titik-titik jalur dari grup jalur
    private void GetPath()
    {
        // Ambil semua objek anak dari pathGroup
        Transform[] path_objs = pathGroup.GetComponentsInChildren<Transform>();
        // Buat array baru untuk jalur
        path = new Transform[path_objs.Length - 1];

        int index = 0;
        // Masukkan semua titik (selain pathGroup itu sendiri) ke dalam array jalur
        for (int i = 0; i < path_objs.Length; i++)
        {
            if (path_objs[i] != pathGroup)
            {
                path[index] = path_objs[i];
                index++;
            }
        }
    }

    // Fungsi untuk menghitung sudut kemudi
    private void GetSteer()
    {
        // Hitung posisi relatif terhadap titik jalur berikutnya
        Vector3 steerVector = transform.InverseTransformPoint(path[currentPathObj].position);
        // Hitung sudut kemudi berdasarkan posisi relatif
        float newSteer = maxSteer * (steerVector.x / steerVector.magnitude);

        // Atur sudut kemudi roda depan
        wheelFL.steerAngle = newSteer;
        wheelFR.steerAngle = newSteer;

        // Jika sudah dekat dengan titik jalur, pindah ke titik berikutnya
        if (steerVector.magnitude <= distFromPath)
        {
            currentPathObj++;
            // Jika sudah di titik terakhir, kembali ke titik pertama
            if (currentPathObj >= path.Length)
                currentPathObj = 0;
        }
    }

    // Fungsi untuk menggerakkan mobil
    private void Move()
    {
        // Hitung kecepatan saat ini berdasarkan putaran roda
        currentSpeed = 2 * (22f / 7f) * wheelRL.radius * wheelRL.rpm * 60f / 1000f;
        currentSpeed = Mathf.Round(currentSpeed);

        // Jika kecepatan kurang dari kecepatan maksimal
        if (currentSpeed <= topSpeed)
        {
            // Tambahkan torsi motor untuk menggerakkan mobil maju
            wheelRL.motorTorque = maxTorque;
            wheelRR.motorTorque = maxTorque;
            wheelRL.brakeTorque = 0f;
            wheelRR.brakeTorque = 0f;
        }
        else
        {
            // Jika kecepatan melebihi batas, terapkan rem
            wheelRL.motorTorque = 0f;
            wheelRR.motorTorque = 0f;
            wheelRL.brakeTorque = decelerationSpeed;
            wheelRR.brakeTorque = decelerationSpeed;
        }
    }
}

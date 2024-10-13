using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Maniabilit�")]
    [Space(5)]
    public float acceleration;
    public float deceleration;
    public float brakes; // Corrected from 'breaks' to 'brakes'
    public float rotationSpeed;
    public float maxSpeed;
    public float grip;

    [Header("Tilt")]
    [Space(5)]
    public float tiltAmount = 15f; // Angle d'inclinaison maximum pour le mod�le principal
    public float tiltSpeed = 5f; // Vitesse de transition pour l'inclinaison
    public float rotationThreshold = 1f; // Seuil de vitesse pour permettre la rotation

    private Rigidbody rb;
    public Transform model_pods; // R�f�rence au mod�le 3D principal (pods)
    public Transform model_seat; // R�f�rence au mod�le 3D secondaire (si�ge)
    public float secondaryTiltAmount = 7.5f; // Angle d'inclinaison pour le mod�le secondaire

    private float currentTilt; // Inclinaison actuelle du mod�le principal
    private float currentSecondaryTilt; // Inclinaison actuelle du mod�le secondaire

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentTilt = 0f; // Initialiser l'inclinaison actuelle
        currentSecondaryTilt = 0f; // Initialiser l'inclinaison actuelle pour le mod�le secondaire
        rb.useGravity = true; // Si vous avez besoin de la gravit�
    }

    void Update()
    {
        // Appliquer la vitesse en fonction de la direction actuelle (forward)
        Vector3 forward = transform.forward;
        Vector3 right = transform.right; // La direction lat�rale (gauche-droite)

        // R�duire la d�rive lat�rale (glisse sur le c�t�)
        Vector3 velocity = rb.velocity;
        float lateralSpeed = Vector3.Dot(velocity, right); // Vitesse sur l'axe lat�ral
        Vector3 lateralVelocity = right * lateralSpeed;

        // Appliquer l'adh�rence : plus la valeur du grip est haute, moins il y a de glisse
        Vector3 correctedVelocity = new Vector3(velocity.x, velocity.y, velocity.z) - lateralVelocity * grip;
        rb.velocity = correctedVelocity; // On applique la v�locit� corrig�e

        // Acc�l�rer en avant (Z)
        if (Input.GetKey(KeyCode.Z))
        {
            if (rb.velocity.magnitude < maxSpeed)
            {
                rb.AddForce(forward * acceleration, ForceMode.Acceleration); // Avancer
            }
        }

        // Reculer (S)
        if (Input.GetKey(KeyCode.S))
        {
            if (rb.velocity.magnitude < maxSpeed)
            {
                // Appliquer une force vers l'arri�re pour le recul
                rb.AddForce(-forward * brakes, ForceMode.Acceleration); // Reculer
            }
        }

        // Gestion de la rotation et de l'inclinaison uniquement si la vitesse est suffisante
        if (rb.velocity.magnitude > rotationThreshold) // V�rifier si la vitesse est sup�rieure au seuil
        {
            if (Input.GetKey(KeyCode.D))
            {
                transform.Rotate(0, rotationSpeed * Time.deltaTime, 0); // Tourner � droite
                // Inclinaison en fonction du temps de rotation et de la vitesse
                currentTilt = Mathf.Lerp(currentTilt, -tiltAmount * (rb.velocity.magnitude / maxSpeed), Time.deltaTime * tiltSpeed);
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                transform.Rotate(0, -rotationSpeed * Time.deltaTime, 0); // Tourner � gauche
                // Inclinaison en fonction du temps de rotation et de la vitesse
                currentTilt = Mathf.Lerp(currentTilt, tiltAmount * (rb.velocity.magnitude / maxSpeed), Time.deltaTime * tiltSpeed);
            }
            else
            {
                // Revenir � une position droite si aucune touche n'est press�e
                currentTilt = Mathf.Lerp(currentTilt, 0, Time.deltaTime * tiltSpeed); // Revenir � plat le mod�le principal
            }
        }
        else
        {
            // Revenir � une position droite si aucune touche n'est press�e et que la vitesse est faible
            currentTilt = Mathf.Lerp(currentTilt, 0, Time.deltaTime * tiltSpeed); // Revenir � plat le mod�le principal
        }

        // Appliquer l'inclinaison sur l'axe Z
        LeanModel(model_pods, currentTilt); // Appliquer l'inclinaison sur Z
        LeanModel(model_seat, currentSecondaryTilt); // Appliquer l'inclinaison sur Z pour le mod�le secondaire

        // D�c�l�rer si aucune touche n'est press�e
        if (!Input.GetKey(KeyCode.Z) && !Input.GetKey(KeyCode.S))
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, deceleration * Time.deltaTime); // Lerp pour une d�c�l�ration douce
        }
    }

    // Fonction pour incliner un mod�le 3D sur l'axe Z (lat�ral)
    void LeanModel(Transform model, float targetTilt)
    {
        // Effectuer une interpolation pour une transition fluide
        Vector3 currentRotation = model.localEulerAngles;
        // Appliquer l'inclinaison sur Z
        model.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, targetTilt); // Appliquer l'inclinaison sur Z
    }
}

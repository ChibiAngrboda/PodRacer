using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Maniabilité")]
    [Space(5)]
    public float acceleration;
    public float deceleration;
    public float brakes; // Corrected from 'breaks' to 'brakes'
    public float rotationSpeed;
    public float maxSpeed;
    public float grip;

    [Header("Tilt")]
    [Space(5)]
    public float tiltAmount = 15f; // Angle d'inclinaison maximum pour le modèle principal
    public float tiltSpeed = 5f; // Vitesse de transition pour l'inclinaison
    public float rotationThreshold = 1f; // Seuil de vitesse pour permettre la rotation

    private Rigidbody rb;
    public Transform model_pods; // Référence au modèle 3D principal (pods)
    public Transform model_seat; // Référence au modèle 3D secondaire (siège)
    public float secondaryTiltAmount = 7.5f; // Angle d'inclinaison pour le modèle secondaire

    private float currentTilt; // Inclinaison actuelle du modèle principal
    private float currentSecondaryTilt; // Inclinaison actuelle du modèle secondaire

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentTilt = 0f; // Initialiser l'inclinaison actuelle
        currentSecondaryTilt = 0f; // Initialiser l'inclinaison actuelle pour le modèle secondaire
        rb.useGravity = true; // Si vous avez besoin de la gravité
    }

    void Update()
    {
        // Appliquer la vitesse en fonction de la direction actuelle (forward)
        Vector3 forward = transform.forward;
        Vector3 right = transform.right; // La direction latérale (gauche-droite)

        // Réduire la dérive latérale (glisse sur le côté)
        Vector3 velocity = rb.velocity;
        float lateralSpeed = Vector3.Dot(velocity, right); // Vitesse sur l'axe latéral
        Vector3 lateralVelocity = right * lateralSpeed;

        // Appliquer l'adhérence : plus la valeur du grip est haute, moins il y a de glisse
        Vector3 correctedVelocity = new Vector3(velocity.x, velocity.y, velocity.z) - lateralVelocity * grip;
        rb.velocity = correctedVelocity; // On applique la vélocité corrigée

        // Accélérer en avant (Z)
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
                // Appliquer une force vers l'arrière pour le recul
                rb.AddForce(-forward * brakes, ForceMode.Acceleration); // Reculer
            }
        }

        // Gestion de la rotation et de l'inclinaison uniquement si la vitesse est suffisante
        if (rb.velocity.magnitude > rotationThreshold) // Vérifier si la vitesse est supérieure au seuil
        {
            if (Input.GetKey(KeyCode.D))
            {
                transform.Rotate(0, rotationSpeed * Time.deltaTime, 0); // Tourner à droite
                // Inclinaison en fonction du temps de rotation et de la vitesse
                currentTilt = Mathf.Lerp(currentTilt, -tiltAmount * (rb.velocity.magnitude / maxSpeed), Time.deltaTime * tiltSpeed);
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                transform.Rotate(0, -rotationSpeed * Time.deltaTime, 0); // Tourner à gauche
                // Inclinaison en fonction du temps de rotation et de la vitesse
                currentTilt = Mathf.Lerp(currentTilt, tiltAmount * (rb.velocity.magnitude / maxSpeed), Time.deltaTime * tiltSpeed);
            }
            else
            {
                // Revenir à une position droite si aucune touche n'est pressée
                currentTilt = Mathf.Lerp(currentTilt, 0, Time.deltaTime * tiltSpeed); // Revenir à plat le modèle principal
            }
        }
        else
        {
            // Revenir à une position droite si aucune touche n'est pressée et que la vitesse est faible
            currentTilt = Mathf.Lerp(currentTilt, 0, Time.deltaTime * tiltSpeed); // Revenir à plat le modèle principal
        }

        // Appliquer l'inclinaison sur l'axe Z
        LeanModel(model_pods, currentTilt); // Appliquer l'inclinaison sur Z
        LeanModel(model_seat, currentSecondaryTilt); // Appliquer l'inclinaison sur Z pour le modèle secondaire

        // Décélérer si aucune touche n'est pressée
        if (!Input.GetKey(KeyCode.Z) && !Input.GetKey(KeyCode.S))
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, deceleration * Time.deltaTime); // Lerp pour une décélération douce
        }
    }

    // Fonction pour incliner un modèle 3D sur l'axe Z (latéral)
    void LeanModel(Transform model, float targetTilt)
    {
        // Effectuer une interpolation pour une transition fluide
        Vector3 currentRotation = model.localEulerAngles;
        // Appliquer l'inclinaison sur Z
        model.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, targetTilt); // Appliquer l'inclinaison sur Z
    }
}

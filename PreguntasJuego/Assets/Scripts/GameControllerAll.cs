using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using models;
using System.IO;
using System;
using TMPro;
using System.Linq;
using UnityEngine.UI;

public class GameControllerPreguntas : MonoBehaviour
{
    //AudioManager
    private AudioManager audioManager;

    // Listas de preguntas
    private List<object> preguntasUsadas = new List<object>();
    private List<PreguntasMultiples> listaPM;
    private List<PreguntasFV> listaFV;
    private List<PreguntasAbiertas> listaPA;

    // Paneles
    [SerializeField] private GameObject panelMultiples;
    [SerializeField] private GameObject panelFV;
    [SerializeField] private GameObject panelAbiertas;
    [SerializeField] public GameObject panelInicio;
    [SerializeField] private GameObject panelFinal;

    // UI inicio
    public Button BInicio;
    public Button BSalir;

    //UI final
    public TextMeshProUGUI txtNroPreguntas;
    public TextMeshProUGUI txtCorrectos;
    public TextMeshProUGUI txtIncorrectos;
    public TextMeshProUGUI txtPorcentaje;
    public Button BReiniciar;


    // UI para múltiples
    [SerializeField] private TextMeshProUGUI txtPreguntaMultiples;
    [SerializeField] private TextMeshProUGUI txtRespuesta1;
    [SerializeField] private TextMeshProUGUI txtRespuesta2;
    [SerializeField] private TextMeshProUGUI txtRespuesta3;
    [SerializeField] private TextMeshProUGUI txtRespuesta4;

    // UI para FV
    [SerializeField] private TextMeshProUGUI txtPreguntaFV;
    [SerializeField] private TextMeshProUGUI txtRespuestaF;
    [SerializeField] private TextMeshProUGUI txtRespuestaV;

    // UI para abiertas
    [SerializeField] private TextMeshProUGUI txtPreguntaAbierta;
    [SerializeField] private TMP_InputField inputRespuestaAbierta;

    // Variables de control
    private string respuestaCorrectaActual;
    private string dificultadActual = "facil";
    private System.Random random = new System.Random();
    private bool cambioRealizado = false;
    private int correctas;
    private int incorrectas;

    void Start()
    {

        audioManager = AudioManager.instance;
        audioManager.PlayMusic(audioManager.menuMusic);
        audioManager = FindObjectOfType<AudioManager>();

        InicializarListas();
        CargarTodasLasPreguntas();
        panelInicio.SetActive(true);
        panelFinal.SetActive(false);
        BInicio.onClick.AddListener(IniciarJuego);
        BSalir.onClick.AddListener(SalirJuego);
        BReiniciar.onClick.AddListener(ReiniciarJuego);
    }

    private void InicializarListas()
    {
        listaPM = new List<PreguntasMultiples>();
        listaFV = new List<PreguntasFV>();
        listaPA = new List<PreguntasAbiertas>();
    }

    void IniciarJuego()
    {
        panelInicio.SetActive(false);
        audioManager.PlayMusic(audioManager.easyMusic);
        GenerarPreguntaAleatoria();
    }

    void SalirJuego()
    {
        Application.Quit();
    }

    private void CargarTodasLasPreguntas()
    {
        LeerPreguntasMultiples();
        LeerPreguntasFV();
        LeerPreguntasAbiertas();
    }

    void StartHardQuestions()
    {
        audioManager.PlayMusic(audioManager.hardMusic);
        GenerarPreguntaAleatoria();
    }

    public void GenerarPreguntaAleatoria()
    {
        DesactivarTodosLosPaneles();

        int totalPreguntas = listaPM.Count + listaFV.Count + listaPA.Count;
        if (preguntasUsadas.Count >= totalPreguntas)
        {
            MostrarPanelFinal();
            return;
        }

        if (dificultadActual == "facil")
        {
            bool quedanFaciles = listaPM.Exists(p => p.Dificultad == "facil" && !preguntasUsadas.Contains(p)) ||
                                 listaFV.Exists(p => p.Dificultad == "facil" && !preguntasUsadas.Contains(p)) ||
                                 listaPA.Exists(p => p.Dificultad == "facil" && !preguntasUsadas.Contains(p));

            if (!quedanFaciles && !cambioRealizado)
            {
                dificultadActual = "dificil";
                cambioRealizado = true;
                StartHardQuestions();
                Debug.Log("Cambiando a preguntas difíciles.");
            }
        }

        List<object> preguntasDisponibles = new List<object>();
        preguntasDisponibles.AddRange(listaPM.Where(p => p.Dificultad == dificultadActual && !preguntasUsadas.Contains(p)));
        preguntasDisponibles.AddRange(listaFV.Where(p => p.Dificultad == dificultadActual && !preguntasUsadas.Contains(p)));
        preguntasDisponibles.AddRange(listaPA.Where(p => p.Dificultad == dificultadActual && !preguntasUsadas.Contains(p)));

        preguntasDisponibles = preguntasDisponibles.OrderBy(p => random.Next()).ToList();

        if (preguntasDisponibles.Count == 0)
        {
            MostrarPanelFinal();
            return;
        }

        object preguntaSeleccionada = preguntasDisponibles.First();
        preguntasUsadas.Add(preguntaSeleccionada);

        if (preguntaSeleccionada is PreguntasMultiples)
            MostrarPreguntaMultiples((PreguntasMultiples)preguntaSeleccionada);
        else if (preguntaSeleccionada is PreguntasFV)
            MostrarPreguntaFV((PreguntasFV)preguntaSeleccionada);
        else if (preguntaSeleccionada is PreguntasAbiertas)
            MostrarPreguntaAbierta((PreguntasAbiertas)preguntaSeleccionada);
    }

    private void DesactivarTodosLosPaneles()
    {
        panelMultiples.SetActive(false);
        panelFV.SetActive(false);
        panelAbiertas.SetActive(false);
    }

    public void CambiarEstrategiaDificultad()
    {
        if (dificultadActual == "facil")
            dificultadActual = "dificil";
        else
            dificultadActual = "facil";

        Debug.Log("Nueva dificultad establecida: " + dificultadActual);
    }

    private void MostrarPreguntaMultiples(PreguntasMultiples pregunta)
    {
        panelMultiples.SetActive(true);
        txtPreguntaMultiples.text = pregunta.Pregunta;
        txtRespuesta1.text = pregunta.Respuesta1;
        txtRespuesta2.text = pregunta.Respuesta2;
        txtRespuesta3.text = pregunta.Respuesta3;
        txtRespuesta4.text = pregunta.Respuesta4;
        respuestaCorrectaActual = pregunta.RespuestaCorrecta.Trim().ToLower();
        dificultadActual = pregunta.Dificultad;
    }

    private void MostrarPreguntaFV(PreguntasFV pregunta)
    {
        panelFV.SetActive(true);
        txtPreguntaFV.text = pregunta.Pregunta;

        txtRespuestaF.text = "Falso";
        txtRespuestaV.text = "Verdadero";

        respuestaCorrectaActual = pregunta.RespuestaCorrecta;
        dificultadActual = pregunta.Dificultad;
    }

    private void MostrarPreguntaAbierta(PreguntasAbiertas pregunta)
    {
        panelAbiertas.SetActive(true);
        txtPreguntaAbierta.text = pregunta.Pregunta;
        inputRespuestaAbierta.text = "";
        respuestaCorrectaActual = pregunta.RespuestaCorrecta;
        dificultadActual = pregunta.Dificultad;
    }

    public void VerificarRespuestaMultiples(int respuestaNum)
    {
        string respuestaSeleccionada = ObtenerRespuestaSeleccionada(respuestaNum);
        bool esCorrecta = respuestaSeleccionada.Equals(respuestaCorrectaActual, StringComparison.OrdinalIgnoreCase);
        MostrarResultado(esCorrecta);
    }

    private string ObtenerRespuestaSeleccionada(int respuestaNum)
    {
        switch (respuestaNum)
        {
            case 1: return txtRespuesta1.text;
            case 2: return txtRespuesta2.text;
            case 3: return txtRespuesta3.text;
            case 4: return txtRespuesta4.text;
            default: return string.Empty;
        }
    }


    public void OnClickFV(int respuestaNum)
    {
        VerificarRespuestaFV(respuestaNum);
    }

    public void VerificarRespuestaFV(int respuestaNum)
    {
        if (string.IsNullOrEmpty(respuestaCorrectaActual))
        {
            return;
        }

        bool respuestaJugador = (respuestaNum == 1);
        string respuestaLimpia = respuestaCorrectaActual.Trim().ToLower();

        if (respuestaLimpia != "verdadero" && respuestaLimpia != "falso" && respuestaLimpia != "true" && respuestaLimpia != "false")
        {
            return;
        }

        bool respuestaCorrectaBool = respuestaLimpia == "verdadero" || respuestaLimpia == "true";

        MostrarResultado(respuestaJugador == respuestaCorrectaBool);
    }

    private void MostrarResultado(bool esCorrecta)
    {
        Debug.Log(esCorrecta ? "¡Correcto!" : "¡Incorrecto!");

        if (esCorrecta)
        {
            correctas++;
            audioManager.PlaySound(audioManager.correctSound);
        }
        else
        {
            incorrectas++;
            audioManager.PlaySound(audioManager.wrongSound);
        }

        int totalPreguntas = listaPM.Count + listaFV.Count + listaPA.Count;
        if (preguntasUsadas.Count >= totalPreguntas)
        {
            MostrarPanelFinal();
        }
        else
        {
            GenerarPreguntaAleatoria();
        }
    }


    public void MostrarResultados()
    {
        int totalPreguntas = correctas + incorrectas;
        float porcentaje = (totalPreguntas > 0) ? (correctas * 100f / totalPreguntas) : 0;

        txtNroPreguntas.text = $"Total Preguntas: {totalPreguntas}";
        txtCorrectos.text = $"Correctas: {correctas}";
        txtIncorrectos.text = $"Incorrectas: {incorrectas}";
        txtPorcentaje.text = $"Aciertos: {porcentaje:F2}%";
    }

    public void MostrarPanelFinal()
    {
        panelFinal.SetActive(true);
        audioManager.PlayMusic(audioManager.finalMusic);
        int totalPreguntas = correctas + incorrectas;
        float porcentaje = (totalPreguntas > 0) ? (correctas * 100f / totalPreguntas) : 0;

        txtNroPreguntas.text = $"Total Preguntas: {totalPreguntas}";
        txtCorrectos.text = $"Correctas: {correctas}";
        txtIncorrectos.text = $"Incorrectas: {incorrectas}";
        txtPorcentaje.text = $"Aciertos: {porcentaje:F2}%";

    }

    public void VerificarRespuestaAbierta()
    {
        string respuestaUsuario = inputRespuestaAbierta.text.Trim();
        bool esCorrecta = respuestaUsuario.Equals(respuestaCorrectaActual, StringComparison.OrdinalIgnoreCase);
        MostrarResultado(esCorrecta);
    }

    private void LeerPreguntasMultiples()
    {
        try
        {
            string path = Path.Combine(Application.dataPath, "Resources/Files/ArchivoPreguntasM.txt");
            using (StreamReader sr = new StreamReader(path))
            {
                string lineaLeida;
                while ((lineaLeida = sr.ReadLine()) != null)
                {
                    ProcesarLineaPreguntaMultiple(lineaLeida);
                }
            }
            Debug.Log($"Preguntas múltiples cargadas: {listaPM.Count}");
        }
        catch (Exception e)
        {
            Debug.LogError("Error leyendo preguntas múltiples: " + e.Message);
        }
    }

    private void ProcesarLineaPreguntaMultiple(string linea)
    {
        string[] partes = linea.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length >= 8)
        {
            PreguntasMultiples pregunta = new PreguntasMultiples(
                partes[0].Trim(), partes[1].Trim(), partes[2].Trim(),
                partes[3].Trim(), partes[4].Trim(), partes[5].Trim(),
                partes[6].Trim(), partes[7].Trim());
            listaPM.Add(pregunta);
        }
    }


    private void LeerPreguntasFV()
    {
        try
        {
            string path = Path.Combine(Application.dataPath, "Resources/Files/preguntasFalso_Verdadero.txt");
            using (StreamReader sr = new StreamReader(path))
            {
                string lineaLeida;
                while ((lineaLeida = sr.ReadLine()) != null)
                {
                    ProcesarLineaPreguntaFV(lineaLeida);
                }
            }
            Debug.Log($"Preguntas F/V cargadas: {listaFV.Count}");
        }
        catch (Exception e)
        {
            Debug.LogError("Error leyendo preguntas F/V: " + e.Message);
        }
    }



    private void ProcesarLineaPreguntaFV(string linea)
    {
        string[] partes = linea.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length >= 4)
        {
            PreguntasFV pregunta = new PreguntasFV(
                partes[0].Trim(),
                partes[1].Trim(),
                partes[2].Trim(),
                partes[3].Trim()  
            );
            listaFV.Add(pregunta);
        }
    }


    private void LeerPreguntasAbiertas()
    {
        try
        {
            string path = Path.Combine(Application.dataPath, "Resources/Files/ArchivoPreguntasAbiertas.txt");
            using (StreamReader sr = new StreamReader(path))
            {
                string lineaLeida;
                while ((lineaLeida = sr.ReadLine()) != null)
                {
                    ProcesarLineaPreguntaAbierta(lineaLeida);
                }
            }
            Debug.Log($"Preguntas abiertas cargadas: {listaPA.Count}");
        }
        catch (Exception e)
        {
            Debug.LogError("Error leyendo preguntas abiertas: " + e.Message);
        }
    }

    private void ProcesarLineaPreguntaAbierta(string linea)
    {
        string[] partes = linea.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length >= 4)
        {
            PreguntasAbiertas pregunta = new PreguntasAbiertas(
                partes[0].Trim(), partes[1].Trim(),
                partes[2].Trim(), partes[3].Trim());
            listaPA.Add(pregunta);
        }
    }
    public void ReiniciarJuego()
    {
        correctas = 0;
        incorrectas = 0;
        preguntasUsadas.Clear();
        dificultadActual = "facil";
        cambioRealizado = false;

        panelFinal.SetActive(false);
        panelInicio.SetActive(true);

        audioManager.PlayMusic(audioManager.menuMusic);
    }
}
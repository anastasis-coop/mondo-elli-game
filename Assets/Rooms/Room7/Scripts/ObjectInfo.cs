using System.Collections.Generic;

namespace Room7
{

  public enum Insieme { Numero, Forma, Colore, Funzione, Dimensione };
  public enum Forma { Nessuna, Cerchio, Cono, Cubo, Rettangolo, Triangolo };
  public enum Colore { Nessuno, Arancione, Azzurro, Bianco, BiancoNero, Blu, Giallo, Grigio, Marrone, Nero, Rosa, Rosso, Verde, Viola };
  public enum Funzione { Nessuna, Animale, Attrezzi, Bere, Cane, Comunicazione, Contenitore, Cucina, Dolci, Festa, Frutta, Meteo, MezziTrasporto, MisuraTempo, Natura, Nido, Scuola, Sport, Verdura, Vestiti };

  public class ObjectInfo {

    public string name;
    public int numberOfObjects = 1;
    public Colore color;
    public List<Colore> avoidColors;
    public Forma shape;
    public List<Forma> avoidShapes;
    public Funzione function;
    public List<Funzione> avoidFunctions;

    public ObjectInfo(string name) {
      this.name = name;
      avoidShapes = new List<Forma>();
      avoidColors = new List<Colore>();
      avoidFunctions = new List<Funzione>();
    }

    public void avoidShape(Forma shapeToAvoid) {
      avoidShapes.Add(shapeToAvoid);
    }

    public void avoidColor(Colore colorToAvoid) {
      avoidColors.Add(colorToAvoid);
    }

    public void avoidFunction(Funzione functionToAvoid) {
      avoidFunctions.Add(functionToAvoid);
    }

  }

}
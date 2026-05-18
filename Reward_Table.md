# ML-Agents Reward Tabel: Prop Hunt (Seeker AI)

Deze tabel is specifiek afgestemd op een Prop Hunt variant, waarbij de AI in een grid-gebaseerde map moet zoeken en objecten bewust moet "aanvallen" of "interacteren" om te checken of het de speler is.

## Beloningsstructuur

| Gebeurtenis                      | Beloning (Float) | Type        | Uitleg                                                                                                                                                  |
| :------------------------------- | :--------------- | :---------- | :------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Hider Gevonden (Aangevallen)** | `+1.0f`          | Terminal    | De AI voert de "aanval" actie uit op het juiste object (de Hider). Roep hierna `EndEpisode()` aan.                                                      |
| **Verkeerde Prop Aangevallen**   | `-0.1f`          | Incidenteel | De AI valt een echt (nep) object aan. Dit voorkomt dat de AI simpelweg elk object in de map spamt.                                                      |
| **Nieuwe Grid Kamer Ontdekt**    | `+0.05f`         | Incidenteel | Alleen de **eerste** keer dat de AI een specifieke kamer binnenstapt in een episode. Moedigt exploratie aan zonder farming.                             |
| **Tijdstraf (Step)**             | `-0.0001f`       | Per Step    | De 'existential penalty'. Dwingt de AI om efficiënt te zoeken in plaats van stil te staan. Zorg dat dit over de hele ronde niet lager dan -1.0 uitkomt. |
| **Muur Botsing (Optioneel)**     | `-0.01f`         | Incidenteel | Straf voor het lopen tegen statische objecten/muren. Zorgt voor soepelere navigatie door het grid.                                                      |

## ⚠️ Essentiële Implementatie Tips voor Prop Hunt

1. **Bewuste Aanval:** Koppel de beloning en minpunten aan een specifieke actie (bijv. `actions.DiscreteActions[1] == 1`). Geef **geen** minpunten als de AI er alleen maar per ongeluk tegenaan loopt.
2. **Gelijke Tags:** Zorg dat de Hider (als prop) exact dezelfde Unity Tag en Layer heeft als de echte props in de kamer. De AI mag het verschil niet kunnen aflezen uit de Raycast, maar moet het leren uit logica!
3. **Geheugen is vereist:** Zet **Memory (LSTM)** aan in je ML-Agents `.yaml` configuratiebestand. De AI moet kunnen onthouden waar objecten stonden om te herkennen of een object verplaatst is of er ineens extra staat.
4. **Houd de Hider in beweging (tijdens training):** Als de Hider altijd op dezelfde plek in dezelfde kamer spawnt, leert de AI gewoon die route uit zijn hoofd. Randomize de verstop-locaties voor elke episode!

---

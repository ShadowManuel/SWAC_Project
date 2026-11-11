import express from 'express';
import Med from '../models/medModel.js';
import { getMedfromAPI, compareWithExistingMedications } from '../controller/MedApi.js';
import { authMiddleware } from '../auth.js';

const router = express.Router();

// Route zum Abrufen von Medikamenten aus der API und Speichern in der Datenbank
router.post('/', authMiddleware, async (req, res) => {
  const { keyword, list_id } = req.body;
  if (!keyword || !list_id) {
    return res.status(400).send('Keyword and list_id are required');
  }
  console.log('Empfangenes Keyword und list_id:', keyword, list_id);

  try {
    const data = await getMedfromAPI(keyword);
    if (!data) {
      return res.status(500).send('Med konnte nicht abgerufen werden');
    }
    console.log('Erhaltene Med-Daten:', data);

    console.log('Extracted MedData:', data.id, data.title); // Loggen des extrahierten Medikaments

    // Neues Medikament in der Liste erstellen
    const newMed = await Med.create({ m_list_id: list_id, med_id: data.id, med_title_en: data.title });

    // Erfolgreiche Antwort mit dem erstellten Medikament zurückgeben
    res.status(201).json(newMed);
  } catch (error) {
    console.error('Fehler beim Abrufen der Medikamente oder beim Speichern in der Datenbank:', error);
    res.status(500).send('Interner medRoute Serverfehler');
  }
});

// Route zum Abrufen aller Medikamente
router.get('/', authMiddleware, async (req, res) => {
  try {
    const { list_id } = req.query;
    if (!list_id) {
      return res.status(400).send('list_id is required');
    }

    const meds = await Med.findAll({ where: { m_list_id: list_id } });
    if (meds.length > 0) {
      res.status(200).json(meds);
    } else {
      res.status(404).json({ error: 'Keine Medikamente gefunden.' });
    }
  } catch (error) {
    console.error('Fehler beim Abrufen der Medikamente:', error);
    res.status(500).json({ error: 'Fehler beim Abrufen der Medikamente.' });
  }
});

// Route zum Abrufen eines spezifischen Medikaments nach ID
router.get('/:id', authMiddleware, async (req, res) => {
  try {
    const med = await Med.findByPk(req.params.id);
    if (med) {
      res.status(200).json(med);
    } else {
      res.status(404).json({ error: 'Med nicht gefunden.' });
    }
  } catch (error) {
    console.error('Fehler beim Abrufen des Meds:', error);
    res.status(500).json({ error: 'Fehler beim Abrufen des Meds.' });
  }
});

// Route zum Vergleich von Medikamenten
router.get('/compare/:newMedId', authMiddleware, async (req, res) => {
  try {
    const newMedId = req.params.newMedId;
    const comparisonResults = await compareWithExistingMedications(newMedId);
    res.status(200).json(comparisonResults);
  } catch (error) {
    console.error('Fehler beim Vergleich der Medikamente:', error);
    res.status(500).json({ error: 'Fehler beim Vergleich der Medikamente.' });
  }
});

// Route zum Löschen eines Medikaments
router.delete('/:id', authMiddleware, async (req, res) => {
  try {
    const deleted = await Med.destroy({
      where: { med_id: req.params.id }
    });
    if (deleted) {
      res.status(204).json({ message: 'Med erfolgreich gelöscht.' });
    } else {
      res.status(404).json({ error: 'Med nicht gefunden.' });
    }
  } catch (error) {
    console.error('Fehler beim Löschen des Meds:', error);
    res.status(500).json({ error: 'Fehler beim Löschen des Meds.' });
  }
});

export default router;

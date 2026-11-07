// backend/routes/itemRoutes.js

import express from 'express';
import Item from '../models/itemModel.js';
import List from '../models/listModel.js';
import { getIconDatafromAPI } from '../controller/IconApi.js';
import { authMiddleware } from '../auth.js';

const router = express.Router();

// Erstellen eines neuen Items
router.post('/', authMiddleware, async (req, res) => {
    const { keyword, listId } = req.body; // listId sollte vom Frontend gesendet werden
    if (!keyword || !listId) {
        return res.status(400).send('Keyword and listId are required');
    }
    console.log('Empfangenes Keyword und listId:', keyword, listId);

    try {
        const iconData = await getIconDatafromAPI(keyword);
        if (!iconData) {
            return res.status(500).send('Icon konnte nicht abgerufen werden');
        }
        console.log('Erhaltene Icon-Daten:', iconData);

        const newItem = await Item.create({ list_id: listId, item_icon: iconData, item_title_en: keyword, item_title_de: keyword });

        // Erfolgreiche Antwort mit dem erstellten Item zurückgeben
        res.status(201).json(newItem);
    } catch (error) {
        console.error('Fehler beim Verarbeiten des Keywords und Icons:', error);
        res.status(500).send('Interner itemRoute-1 Serverfehler');
    }
});

// Route zum Abrufen aller Items
router.get('/', authMiddleware, async (req, res) => {
    try {
        const { list_id } = req.query;
        if (!list_id) {
            return res.status(400).send('list_id is required');
        }

        // Alle Items aus der Datenbank abrufen
        const items = await Item.findAll({ where: { list_id } });

        if (items.length > 0) {
            res.status(200).json(items);
        } else {
            res.status(404).json({ error: 'Keine Items gefunden.' });
        }
    } catch (error) {
        console.error('Fehler beim Abrufen der Items:', error);
        res.status(500).json({ error: 'Interner itemRoute-2 Serverfehler.' });
    }
});

// Abrufen eines Items nach ID
router.get('/:id', authMiddleware, async (req, res) => {
    try {
        const item = await Item.findByPk(req.params.id);
        if (item) {
            res.status(200).json(item);
        } else {
            res.status(404).json({ error: 'Item nicht gefunden.' });
        }
    } catch (error) {
        res.status(500).json({ error: 'Fehler beim Abrufen des Items.' });
    }
});

router.put('/:id', authMiddleware, async (req, res) => {
    try {
        const { icon } = req.body;
        const [updated] = await Item.update({ icon }, {
            where: { id: req.params.id }
        });
        if (updated) {
            const updatedItem = await Item.findByPk(req.params.id);
            res.status(200).json(updatedItem);
        } else {
            res.status(404).json({ error: 'Item nicht gefunden.' });
        }
    } catch (error) {
        res.status(500).json({ error: 'Fehler beim Aktualisieren des Items.' });
    }
});

// Löschen eines Items
router.delete('/:id', authMiddleware, async (req, res) => {
    try {
        const deleted = await Item.destroy({
            where: { id: req.params.id }
        });
        if (deleted) {
            res.status(204).json({ message: 'Item erfolgreich gelöscht.' });
        } else {
            res.status(404).json({ error: 'Item nicht gefunden.' });
        }
    } catch (error) {
        res.status(500).json({ error: 'Fehler beim Löschen des Items.' });
    }
});

export default router;

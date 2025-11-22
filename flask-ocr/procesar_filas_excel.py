import easyocr
import cv2
import spacy
import re
from unidecode import unidecode
from difflib import get_close_matches
import os
import sys
import pandas as pd


reader = easyocr.Reader(['es'])
nlp = spacy.load("es_core_news_sm")

class SmartNameValidator:
    def __init__(self):
        self.common_names = {
            'isabella', 'dompe', 'estrada', 'jesus', 'abraham', 'silva', 'ampre',
            'david', 'orlando', 'mena', 'valverde', 'yadder', 'fernando', 'torres',
            'eduardo', 'domingo', 'zeledon', 'mercado', 'roman', 'alfonso', 'grios',
            'boza', 'bryan', 'alexander', 'cano', 'hotep', 'antonio', 'ruiz', 'lezama',
            'cristina', 'jozabed', 'carvajal', 'ronier', 'jose', 'rivera'
        }
    def learn_from_text(self, text):
        words = [unidecode(w).lower() for w in re.findall(r'\b[A-Za-zÁÉÍÓÚÜÑáéíóúüñ]+\b', text)]
        for word in words:
            if len(word) > 3 and word not in self.common_names:
                self.common_names.add(word)
    def is_valid_name(self, word):
        normalized = unidecode(word).lower()
        if normalized in self.common_names:
            return True
        matches = get_close_matches(normalized, list(self.common_names), n=1, cutoff=0.7)
        return len(matches) > 0

def extract_text(image_path):
    img = cv2.imread(image_path)
    if img is None:
        raise ValueError("Error: No se pudo cargar la imagen")
    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
    results = reader.readtext(
        gray,
        paragraph=True,
        decoder='beamsearch',
        beamWidth=10
    )
    if not results:
        return ""
    return " ".join([res[1] for res in results])

def intelligent_name_extraction(text, validator):
    doc = nlp(text)
    names = []
    for ent in doc.ents:
        if ent.label_ == "PER":
            names.append(ent.text)
    if not names:
        for token in doc:
            if token.pos_ == "PROPN" and token.text.istitle():
                if validator.is_valid_name(token.text):
                    names.append(token.text)
    if not names:
        candidates = re.findall(r'\b[A-ZÁÉÍÓÚÜÑ][a-záéíóúüñ]+\b', text)
        for candidate in candidates:
            if validator.is_valid_name(candidate):
                names.append(candidate)
    if not names:
        names = [text]
    validator.learn_from_text(" ".join(names))
    return " ".join(names)

# Agrega esta función para mejor detección de datos académicos
def extract_academic_data(text):
    """
    Extrae información académica del texto
    """
    data = {
        'nombre': '',
        'grado': '',
        'materia': '',
        'calificacion': '',
        'periodo': ''
    }

    # Buscar patrones comunes en documentos académicos
    lines = text.split('\n')

    for line in lines:
        line_lower = line.lower()

        # Detectar nombre (líneas que contienen solo texto)
        if not data['nombre'] and re.match(r'^[A-Za-zÁÉÍÓÚáéíóúñÑ\s.,]+$', line.strip()):
            if len(line.strip()) > 3:
                data['nombre'] = line.strip()

        # Detectar grado
        if 'grado' in line_lower or 'grade' in line_lower:
            match = re.search(r'(\d+)(?:°|º|o)?', line)
            if match:
                data['grado'] = match.group(1)

        # Detectar calificación
        if 'calificación' in line_lower or 'nota' in line_lower or 'score' in line_lower:
            match = re.search(r'(\d{1,3})', line)
            if match:
                data['calificacion'] = match.group(1)

        # Detectar materia
        materias = ['matemática', 'español', 'ciencia', 'historia', 'geografía', 'inglés',
                    'matematica', 'espanol', 'sociales', 'naturales']
        for materia in materias:
            if materia in line_lower:
                data['materia'] = line.strip()
                break

    return data

def process_image(image_path, validator):
    print(f"\nProcesando: {image_path}")
    try:
        raw_text = extract_text(image_path)
        print(f"Texto detectado: {raw_text}")
        name = intelligent_name_extraction(raw_text, validator)
        print(f"Nombre identificado: {name}")
        return name
    except Exception as e:
        print(f"Error: {str(e)}")
        return ""

def crear_plantilla_si_vacia(excel_path):
    ejemplo = [
        {"archivo": "ejemplo1.png", "nombre_detectado": "Ejemplo Nombre 1"},
        {"archivo": "ejemplo2.png", "nombre_detectado": "Ejemplo Nombre 2"},
        {"archivo": "ejemplo3.png", "nombre_detectado": "Ejemplo Nombre 3"},
    ]
    df = pd.DataFrame(ejemplo)
    df.to_excel(excel_path, index=False)

if __name__ == "__main__":
    filas_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), "filas")
    if len(sys.argv) > 1:
        output_excel = sys.argv[1]
    else:
        output_excel = os.path.join(os.path.dirname(os.path.abspath(__file__)), "resultados_filas.xlsx")
    validator = SmartNameValidator()

    if not os.path.exists(filas_dir):
        print(f"No existe la carpeta de filas: {filas_dir}")
        exit(1)


    if not os.path.exists(output_excel) or os.path.getsize(output_excel) < 100:
        crear_plantilla_si_vacia(output_excel)


    for filename in sorted(os.listdir(filas_dir)):
        if filename.lower().endswith(('.png', '.jpg', '.jpeg')):
            image_path = os.path.join(filas_dir, filename)
            nombre = process_image(image_path, validator)

            try:
                df = pd.read_excel(output_excel)
            except Exception as e:
                print(f"Error leyendo el Excel, se crea plantilla: {e}")
                crear_plantilla_si_vacia(output_excel)
                df = pd.read_excel(output_excel)

            df = pd.concat([df, pd.DataFrame([{"archivo": filename, "nombre_detectado": nombre}])], ignore_index=True)

            df.to_excel(output_excel, index=False)
            print(f"Fila agregada al Excel: {filename} -> {nombre}")

    print(f"\nResultados guardados en: {output_excel}")
    if os.path.exists(output_excel):
        print(f"Excel generado correctamente: {output_excel}")
        print(f"Tamaño del archivo: {os.path.getsize(output_excel)} bytes")
    else:
        print("ERROR: No se generó el archivo Excel")
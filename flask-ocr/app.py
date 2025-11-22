from flask import Flask, request, jsonify, send_from_directory
from flask_cors import CORS
import cv2
import numpy as np
import os
import uuid
import subprocess
import pandas as pd
import base64
from io import BytesIO
from PIL import Image
import shutil
import time

app = Flask(__name__)
CORS(app)

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
UPLOAD_FOLDER = os.path.join(BASE_DIR, 'uploads')
OUTPUT_FOLDER = os.path.join(BASE_DIR, 'filas')
ASISTENCIAS_FOLDER = os.path.join(BASE_DIR, 'asistencias')

# Crear directorios si no existen
for folder in [UPLOAD_FOLDER, OUTPUT_FOLDER, ASISTENCIAS_FOLDER]:
    if not os.path.exists(folder):
        os.makedirs(folder)
        print(f"Carpeta creada: {folder}")
    else:
        print(f"Carpeta ya existe: {folder}")

print(f"BASE_DIR: {BASE_DIR}")
print(f"UPLOAD_FOLDER: {UPLOAD_FOLDER}")
print(f"OUTPUT_FOLDER: {OUTPUT_FOLDER}")

python_process = None

@app.route('/segmentar', methods=['POST'])
def segmentar():
    global python_process
    print("Petición recibida en /segmentar")

    try:
        # Limpiar carpeta filas
        for fname in os.listdir(OUTPUT_FOLDER):
            fpath = os.path.join(OUTPUT_FOLDER, fname)
            try:
                if os.path.isfile(fpath):
                    os.remove(fpath)
                    print(f"Archivo eliminado de filas: {fpath}")
            except Exception as e:
                print(f"Error eliminando {fpath}: {e}")
    except Exception as e:
        print(f"Error limpiando carpeta filas: {e}")

    if 'imagen' not in request.files:
        print("No se envió ninguna imagen")
        return jsonify({'error': 'No se envió ninguna imagen'}), 400

    file = request.files['imagen']
    filename = str(uuid.uuid4()) + '.png'
    filepath = os.path.join(UPLOAD_FOLDER, filename)
    file.save(filepath)
    print(f"Imagen guardada temporalmente en: {filepath}")

    img = cv2.imread(filepath)
    if img is None:
        print(f"Error: No se pudo cargar la imagen '{filepath}'")
        return jsonify({'error': 'No se pudo cargar la imagen'}), 400

    # Procesamiento de imagen para detectar filas
    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
    blur = cv2.GaussianBlur(gray, (5, 5), 0)

    # Detectar si es manuscrito o impreso
    is_handwritten = np.mean(cv2.Laplacian(blur, cv2.CV_64F).var()) < 100

    if is_handwritten:
        thresh = cv2.adaptiveThreshold(blur, 255, cv2.ADAPTIVE_THRESH_GAUSSIAN_C, cv2.THRESH_BINARY_INV, 51, 15)
        kernel = cv2.getStructuringElement(cv2.MORPH_RECT, (50, 1))
    else:
        thresh = cv2.adaptiveThreshold(blur, 255, cv2.ADAPTIVE_THRESH_MEAN_C, cv2.THRESH_BINARY_INV, 15, 7)
        kernel = cv2.getStructuringElement(cv2.MORPH_RECT, (30, 1))

    dilated = cv2.dilate(thresh, kernel, iterations=1)
    horizontal_sum = np.sum(dilated, axis=1)
    threshold = np.max(horizontal_sum) * 0.05
    peaks = np.where(horizontal_sum > threshold)[0]

    if len(peaks) == 0:
        _, thresh_otsu = cv2.threshold(blur, 0, 255, cv2.THRESH_BINARY_INV + cv2.THRESH_OTSU)
        dilated_otsu = cv2.dilate(thresh_otsu, kernel, iterations=1)
        horizontal_sum = np.sum(dilated_otsu, axis=1)
        threshold = np.max(horizontal_sum) * 0.05
        peaks = np.where(horizontal_sum > threshold)[0]

    # Encontrar filas
    rows = []
    if len(peaks) > 0:
        start = peaks[0]
        for i in range(1, len(peaks)):
            if peaks[i] - peaks[i-1] > 10:
                rows.append((start, peaks[i-1]))
                start = peaks[i]
        rows.append((start, peaks[-1]))

    final_rows = []
    for y1, y2 in rows:
        height = y2 - y1
        margin = max(int(height * 0.5), 10)
        y1_adj = max(0, y1 - margin)
        y2_adj = min(img.shape[0], y2 + margin)
        if (y2_adj - y1_adj) > 15:
            final_rows.append((y1_adj, y2_adj))

    print(f"Filas detectadas: {len(final_rows)}")

    fila_urls = []
    BASE_URL = "http://localhost:5000/"

    if not final_rows:
        print("No se detectaron filas, guardando imagen completa")
        fila_filename = f"{uuid.uuid4()}_fila_1.png"
        fila_path = os.path.join(OUTPUT_FOLDER, fila_filename)
        cv2.imwrite(fila_path, img)
        fila_urls.append(BASE_URL + f'fila/{fila_filename}')
    else:
        for idx, (y1, y2) in enumerate(final_rows):
            row_img = img[y1:y2, :]
            fila_filename = f"{uuid.uuid4()}_fila_{idx+1}.png"
            fila_path = os.path.join(OUTPUT_FOLDER, fila_filename)
            cv2.imwrite(fila_path, row_img)
            fila_urls.append(BASE_URL + f'fila/{fila_filename}')

    # Procesar OCR en las filas
    nombres_detectados = []
    try:
        import easyocr
        reader = easyocr.Reader(['es'])

        for fname in os.listdir(OUTPUT_FOLDER):
            if fname.lower().endswith(('.png', '.jpg', '.jpeg')):
                img_path = os.path.join(OUTPUT_FOLDER, fname)
                img_ocr = cv2.imread(img_path)
                if img_ocr is not None:
                    gray_ocr = cv2.cvtColor(img_ocr, cv2.COLOR_BGR2GRAY)
                    results = reader.readtext(gray_ocr, paragraph=True)
                    if results:
                        texto = " ".join([res[1] for res in results])
                        if texto.strip():
                            nombres_detectados.append(texto.strip())
    except Exception as e:
        print(f"Error en OCR: {e}")

    return jsonify({
        'filas': fila_urls,
        'excel': f"{BASE_URL}fila/resultados_filas.xlsx",
        'nombresDetectados': nombres_detectados
    })

@app.route('/fila/<filename>')
def servir_fila(filename):
    return send_from_directory(OUTPUT_FOLDER, filename)

@app.route('/ocr/process', methods=['POST'])
def ocr_process():
    """
    Endpoint para el frontend .NET que espera base64
    """
    try:
        data = request.get_json()
        if not data or 'imageBase64' not in data:
            return jsonify({'success': False, 'error': 'No image data provided'}), 400

        print("Procesando imagen desde base64...")

        # Convertir base64 a imagen
        image_data = base64.b64decode(data['imageBase64'])
        image = Image.open(BytesIO(image_data))

        # Guardar archivo temporal
        temp_filename = f"temp_{uuid.uuid4()}.png"
        temp_path = os.path.join(UPLOAD_FOLDER, temp_filename)
        image.save(temp_path, 'PNG')

        # Procesar con la lógica existente de segmentar
        with open(temp_path, 'rb') as f:
            files = {'imagen': (temp_filename, f, 'image/png')}

            with app.test_client() as client:
                response = client.post('/segmentar', data=files, content_type='multipart/form-data')

        # Limpiar archivo temporal
        try:
            os.remove(temp_path)
        except:
            pass

        if response.status_code != 200:
            return jsonify({'success': False, 'error': 'OCR processing failed'}), 500

        result_data = response.get_json()

        # Formatear respuesta para .NET API
        nombres = result_data.get('nombresDetectados', [])
        full_text = '\n'.join(nombres) if nombres else ''

        return jsonify({
            'success': True,
            'extractedText': full_text,
            'data': {
                'student': {
                    'fullName': nombres[0] if nombres else '',
                    'grade': '',
                    'section': ''
                },
                'academic': {
                    'subject': '',
                    'score': 0,
                    'period': '',
                    'schoolYear': ''
                }
            }
        })

    except Exception as e:
        print(f"Error en ocr/process: {e}")
        return jsonify({'success': False, 'error': str(e)}), 500

@app.route('/health', methods=['GET'])
def health_check():
    return jsonify({'status': 'healthy', 'service': 'flask-ocr'})

# Limpiar carpeta al inicio
def limpiar_carpeta_filas():
    try:
        if os.path.exists(OUTPUT_FOLDER):
            for fname in os.listdir(OUTPUT_FOLDER):
                fpath = os.path.join(OUTPUT_FOLDER, fname)
                if os.path.isfile(fpath):
                    os.remove(fpath)
            print("Carpeta 'filas' limpiada al inicio")
    except Exception as e:
        print(f"Error limpiando carpeta filas: {e}")

limpiar_carpeta_filas()

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True)
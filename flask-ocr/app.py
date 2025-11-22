from flask import Flask, request, jsonify, send_from_directory
from flask_cors import CORS
import cv2
import numpy as np
import os
import uuid
import subprocess
from flask import g
import pandas as pd

app = Flask(__name__)
CORS(app)

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
UPLOAD_FOLDER = os.path.join(BASE_DIR, 'uploads')
OUTPUT_FOLDER = os.path.join(BASE_DIR, 'filas')
ASISTENCIAS_FOLDER = os.path.join(BASE_DIR, 'asistencias')

try:
    if not os.path.exists(UPLOAD_FOLDER):
        os.makedirs(UPLOAD_FOLDER)
        print(f"Carpeta creada: {UPLOAD_FOLDER}")
    else:
        print(f"Carpeta ya existe: {UPLOAD_FOLDER}")
    if not os.path.exists(OUTPUT_FOLDER):
        os.makedirs(OUTPUT_FOLDER)
        print(f"Carpeta creada: {OUTPUT_FOLDER}")
    else:
        print(f"Carpeta ya existe: {OUTPUT_FOLDER}")
    if not os.path.exists(ASISTENCIAS_FOLDER):
        os.makedirs(ASISTENCIAS_FOLDER)
        print(f"Carpeta creada: {ASISTENCIAS_FOLDER}")
    else:
        print(f"Carpeta ya existe: {ASISTENCIAS_FOLDER}")
except Exception as e:
    print(f"ERROR creando carpetas: {e}")

print(f"BASE_DIR: {BASE_DIR}")
print(f"UPLOAD_FOLDER: {UPLOAD_FOLDER}")
print(f"OUTPUT_FOLDER: {OUTPUT_FOLDER}")

python_process = None

@app.route('/segmentar', methods=['POST'])
def segmentar():
    global python_process
    print("Petición recibida en /segmentar")
    print(f"UPLOAD_FOLDER: {UPLOAD_FOLDER}")
    print(f"OUTPUT_FOLDER: {OUTPUT_FOLDER}")
    print(f"Contenido de BASE_DIR: {os.listdir(BASE_DIR)}")

    try:
        import shutil
        intentos = 0
        while True:
            archivos = os.listdir(OUTPUT_FOLDER)
            if not archivos or intentos > 5:
                break
            for fname in archivos:
                fpath = os.path.join(OUTPUT_FOLDER, fname)
                try:
                    if os.path.isfile(fpath) or os.path.islink(fpath):
                        os.remove(fpath)
                        print(f"Archivo eliminado de filas: {fpath}")
                    elif os.path.isdir(fpath):
                        shutil.rmtree(fpath)
                        print(f"Carpeta eliminada de filas: {fpath}")
                except Exception as e:
                    print(f"Error eliminando {fpath}: {e}")
            intentos += 1
        print(f"Contenido de 'filas' después de limpiar: {os.listdir(OUTPUT_FOLDER)}")
    except Exception as e:
        print(f"Error eliminando contenido de filas: {e}")

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

    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
    blur = cv2.GaussianBlur(gray, (5, 5), 0)
    is_handwritten = np.mean(cv2.Laplacian(blur, cv2.CV_64F).var() < 100)
    if is_handwritten:
        thresh = cv2.adaptiveThreshold(
            blur, 255, cv2.ADAPTIVE_THRESH_GAUSSIAN_C,
            cv2.THRESH_BINARY_INV, 51, 15
        )
        kernel = cv2.getStructuringElement(cv2.MORPH_RECT, (50, 1))
    else:
        thresh = cv2.adaptiveThreshold(
            blur, 255, cv2.ADAPTIVE_THRESH_MEAN_C,
            cv2.THRESH_BINARY_INV, 15, 7
        )
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
    rows = []
    start = peaks[0] if len(peaks) > 0 else 0
    for i in range(1, len(peaks)):
        if peaks[i] - peaks[i-1] > 10:
            rows.append((start, peaks[i-1]))
            start = peaks[i]
    if len(peaks) > 0:
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
    print(f"Ruta absoluta de la carpeta 'filas': {OUTPUT_FOLDER}")

    fila_urls = []
    BASE_URL = "http://192.168.1.8:5000/"


    if not final_rows:
        print("No se detectaron filas, guardando imagen completa como fila_1.png")
        fila_filename = f"{uuid.uuid4()}_fila_1.png"
        fila_path = os.path.join(OUTPUT_FOLDER, fila_filename)
        try:
            success = cv2.imwrite(fila_path, img)
            print(f"Imagen completa guardada en: {fila_path}, éxito: {success}, existe: {os.path.exists(fila_path)}")
        except Exception as e:
            print(f"Error guardando imagen: {e}")
        fila_urls.append(BASE_URL + f'fila/{fila_filename}')
    else:
        for idx, (y1, y2) in enumerate(final_rows):
            row_img = img[y1:y2, :]
            fila_filename = f"{uuid.uuid4()}_fila_{idx+1}.png"
            fila_path = os.path.join(OUTPUT_FOLDER, fila_filename)
            try:
                success = cv2.imwrite(fila_path, row_img)
                print(f"Fila guardada: {fila_path}, éxito: {success}, existe: {os.path.exists(fila_path)}")
            except Exception as e:
                print(f"Error guardando fila: {e}")
            fila_urls.append(BASE_URL + f'fila/{fila_filename}')
    try:
        print(f"Archivos en 'filas': {os.listdir(OUTPUT_FOLDER)}")
    except Exception as e:
        print(f"Error listando archivos en 'filas': {e}")


    import time
    max_espera = 5
    espera = 0
    while espera < max_espera:
        archivos = [f for f in os.listdir(OUTPUT_FOLDER) if f.lower().endswith(('.png', '.jpg', '.jpeg'))]
        if archivos:
            break
        time.sleep(0.5)
        espera += 0.5
    print(f"Archivos en 'filas' antes de procesar Excel: {os.listdir(OUTPUT_FOLDER)}")


    excel_filename = "resultados_filas.xlsx"
    excel_path = os.path.join(OUTPUT_FOLDER, excel_filename)
    try:
        print("Procesando filas y generando Excel (interpretando)...")
        python_process = subprocess.Popen(
            ["python", os.path.join(BASE_DIR, "procesar_filas_excel.py"), excel_path],
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True
        )
        stdout, stderr = python_process.communicate()
        print("Procesamiento de filas y Excel completado.")
        print("Salida del script de procesamiento:")
        print(stdout)
        if stderr:
            print("Errores del script de procesamiento:")
            print(stderr)

        if not os.path.exists(excel_path):
            print(f"ERROR: El archivo Excel no se creó en {excel_path}")
        else:
            print(f"Excel generado correctamente en {excel_path}")
        python_process = None
    except Exception as e:
        python_process = None
        print(f"Error al procesar filas y generar Excel: {e}")


    nombres_detectados = []

    import easyocr
    reader = easyocr.Reader(['es'])
    for fname in os.listdir(OUTPUT_FOLDER):
        if fname.lower().endswith(('.png', '.jpg', '.jpeg')):
            img_path = os.path.join(OUTPUT_FOLDER, fname)
            try:
                img = cv2.imread(img_path)
                if img is not None:
                    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
                    results = reader.readtext(gray, paragraph=True)
                    if results:
                        texto = " ".join([res[1] for res in results])
                        if texto.strip():
                            nombres_detectados.append(texto.strip())
            except Exception as e:
                print(f"Error OCR en {fname}: {e}")

    return jsonify({
        'filas': fila_urls,
        'excel': f"{BASE_URL}fila/{excel_filename}",
        'nombresDetectados': nombres_detectados
    })

@app.route('/fila/<filename>')
def fila(filename):
    file_path = os.path.join(OUTPUT_FOLDER, filename)
    print(f"Intentando servir: {file_path} - Existe: {os.path.exists(file_path)}")
    if not os.path.exists(file_path):
        return "Archivo no encontrado", 404
    return send_from_directory(OUTPUT_FOLDER, filename)

@app.route('/asistencias/<filename>')
def descargar_asistencia(filename):
    excel_path = os.path.join(ASISTENCIAS_FOLDER, filename)
    print(f"Intentando servir Excel: {excel_path} - Existe: {os.path.exists(excel_path)}")
    if not os.path.exists(excel_path):
        return "Archivo Excel no encontrado", 404
    return send_from_directory(ASISTENCIAS_FOLDER, filename)

@app.route('/asistencias/<filename>', methods=['DELETE'])
def borrar_asistencia(filename):
    excel_path = os.path.join(ASISTENCIAS_FOLDER, filename)
    if os.path.exists(excel_path):
        try:
            os.remove(excel_path)
            print(f"Excel eliminado: {excel_path}")
            return jsonify({'ok': True})
        except Exception as e:
            return jsonify({'ok': False, 'error': str(e)}), 500
    return jsonify({'ok': False, 'error': 'Archivo no encontrado'}), 404

@app.route('/limpiar-filas', methods=['POST'])
def limpiar_filas():
    try:
        import shutil

        for fname in os.listdir(OUTPUT_FOLDER):
            fpath = os.path.join(OUTPUT_FOLDER, fname)
            try:
                if os.path.isfile(fpath) or os.path.islink(fpath):
                    os.chmod(fpath, 0o777)
                    os.remove(fpath)
                    print(f"Archivo eliminado de filas: {fpath}")
                elif os.path.isdir(fpath):
                    shutil.rmtree(fpath, ignore_errors=True)
                    print(f"Carpeta eliminada de filas: {fpath}")
            except Exception as e:
                print(f"Error eliminando {fpath}: {e}")
        print(f"Contenido final de 'filas': {os.listdir(OUTPUT_FOLDER)}")
        return jsonify({'ok': True})
    except Exception as e:
        print(f"Error limpiando carpeta filas: {e}")
        return jsonify({'ok': False, 'error': str(e)}), 500

@app.route('/eventos', methods=['GET'])
def listar_eventos():
    archivos = []
    BASE_URL = "http://192.168.1.8:5000/"
    for fname in os.listdir(ASISTENCIAS_FOLDER):
        if fname.lower().endswith('.xlsx'):
            archivos.append({
                "nombre": fname,
                "url": f"{BASE_URL}asistencias/{fname}"
            })
    archivos.sort(key=lambda x: os.path.getmtime(os.path.join(ASISTENCIAS_FOLDER, x["nombre"])), reverse=True)
    return jsonify({"eventos": archivos})

@app.route('/cancelar-procesamiento', methods=['POST'])
def cancelar_procesamiento():
    global python_process
    if python_process and python_process.poll() is None:
        try:
            python_process.terminate()
            python_process = None
            print("Procesamiento de Python cancelado por el usuario.")
            return jsonify({'ok': True, 'msg': 'Procesamiento cancelado'})
        except Exception as e:
            return jsonify({'ok': False, 'error': str(e)}), 500
    return jsonify({'ok': False, 'msg': 'No hay proceso en ejecución'}), 400


import shutil


def limpiar_carpeta_filas_al_inicio():
    try:
        if os.path.exists(OUTPUT_FOLDER):
            for fname in os.listdir(OUTPUT_FOLDER):
                fpath = os.path.join(OUTPUT_FOLDER, fname)
                try:
                    if os.path.isfile(fpath) or os.path.islink(fpath):
                        os.remove(fpath)
                    elif os.path.isdir(fpath):
                        shutil.rmtree(fpath)
                except Exception as e:
                    print(f"Error eliminando {fpath}: {e}")
            print("Carpeta 'filas' vaciada al iniciar la app.")
    except Exception as e:
        print(f"Error limpiando carpeta filas al inicio: {e}")


limpiar_carpeta_filas_al_inicio()

@app.route('/crear-asistencia', methods=['POST'])
def crear_asistencia():
    """
    Espera JSON: { "nombre": "nombre_final.xlsx", "nombres": ["Nombre 1", "Nombre 2", ...], "fecha": "YYYY-MM-DD" }
    Crea el excel en asistencias con todos los nombres y un check por defecto.
    """
    data = request.get_json()
    nombre = data.get('nombre')
    nombres = data.get('nombres')
    fecha = data.get('fecha')
    if not nombre or not nombre.endswith('.xlsx'):
        return jsonify({'ok': False, 'error': 'Nombre de archivo inválido'}), 400
    if not isinstance(nombres, list) or not nombres:
        return jsonify({'ok': False, 'error': 'Lista de nombres inválida'}), 400
    if not fecha:
        return jsonify({'ok': False, 'error': 'Fecha requerida'}), 400
    destino = os.path.join(ASISTENCIAS_FOLDER, nombre)
    if os.path.exists(destino):
        return jsonify({'ok': False, 'error': 'Ya existe un archivo con ese nombre'}), 409
    df = pd.DataFrame({
        'N°': list(range(1, len(nombres) + 1)),
        'Nombre': nombres,
        fecha: ['✓'] * len(nombres)
    })
    try:
        df.to_excel(destino, index=False)
        return jsonify({'ok': True, 'url': f"/asistencias/{nombre}"})
    except Exception as e:
        return jsonify({'ok': False, 'error': str(e)}), 500

@app.route('/asistencias/<filename>', methods=['PUT'])
def sobrescribir_asistencia(filename):
    excel_path = os.path.join(ASISTENCIAS_FOLDER, filename)
    if 'file' not in request.files:
        return jsonify({'error': 'No se envió ningún archivo'}), 400
    file = request.files['file']
    file.save(excel_path)
    if os.path.exists(excel_path):
        print(f"Archivo guardado en: {excel_path}")
        print(f"Tamaño del archivo: {os.path.getsize(excel_path)} bytes")
    else:
        print(f"Error: No se guardó el archivo en {excel_path}")
    return jsonify({'ok': True})

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)
    app.run(host='0.0.0.0', port=5000)
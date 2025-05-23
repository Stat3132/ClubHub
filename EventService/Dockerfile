FROM python:latest

WORKDIR /app

# Update system packages to address vulnerabilities
# RUN apt-get update && apt-get upgrade -y && apt-get clean && rm -rf /var/lib/apt/lists/*

COPY requirements.txt .


# Clean cache and uninstall both variants
RUN pip uninstall -y py_eureka_client py-eureka-client || true && \
    pip cache purge && \
    pip install --no-cache-dir -r requirements.txt

COPY . .

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]

class ApiClient {
  constructor(baseUrl) {
    this.baseUrl = baseUrl;
  }

  async request(path, options = {}) {
    const response = await fetch(`${this.baseUrl}${path}`, {
      headers: { "Content-Type": "application/json", ...(options.headers || {}) },
      ...options
    });

    if (response.status === 204) {
      return null;
    }

    const data = await response.json();
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${JSON.stringify(data)}`);
    }

    return data;
  }
}

class SessionService {
  static storageKey = "mira-demo-session";

  save(session) {
    localStorage.setItem(SessionService.storageKey, JSON.stringify(session));
  }

  get() {
    const raw = localStorage.getItem(SessionService.storageKey);
    return raw ? JSON.parse(raw) : null;
  }

  clear() {
    localStorage.removeItem(SessionService.storageKey);
  }
}

class SedesService {
  constructor(apiClient) {
    this.api = apiClient;
  }

  listar() {
    return this.api.request("/api/sedes");
  }

  obtener(id) {
    return this.api.request(`/api/sedes/${id}`);
  }

  crear(payload) {
    return this.api.request("/api/sedes", {
      method: "POST",
      body: JSON.stringify(payload)
    });
  }

  actualizar(id, payload) {
    return this.api.request(`/api/sedes/${id}`, {
      method: "PUT",
      body: JSON.stringify(payload)
    });
  }

  eliminar(id) {
    return this.api.request(`/api/sedes/${id}`, {
      method: "DELETE"
    });
  }
}

class CatalogosService {
  constructor(apiClient) {
    this.api = apiClient;
  }

  listar(tipo) {
    return this.api.request(`/api/catalogos/${tipo}`);
  }

  crear(tipo, payload) {
    return this.api.request(`/api/catalogos/${tipo}`, {
      method: "POST",
      body: JSON.stringify(payload)
    });
  }
}

class PublicoService {
  constructor(apiClient) {
    this.api = apiClient;
  }

  health() {
    return this.api.request("/health");
  }

  resumen() {
    return this.health();
  }
}

class PlatformShellController {
  constructor(sessionService) {
    this.sessionService = sessionService;
    this.userChip = document.getElementById("userChip");
    this.sessionOut = document.getElementById("sessionOut");
    this.loginBtn = document.getElementById("btnDemoLogin");
    this.logoutBtn = document.getElementById("btnDemoLogout");
    this.nameInput = document.getElementById("demoUserName");
    this.roleInput = document.getElementById("demoUserRole");

    this.menuButtons = Array.from(document.querySelectorAll(".menu-btn"));
    this.views = {
      dashboard: document.getElementById("view-dashboard"),
      sedes: document.getElementById("view-sedes"),
      catalogos: document.getElementById("view-catalogos")
    };

    this.onSessionChanged = null;
  }

  wireEvents() {
    this.loginBtn.addEventListener("click", () => this.login());
    this.logoutBtn.addEventListener("click", () => this.logout());

    this.menuButtons.forEach((button) => {
      button.addEventListener("click", () => {
        this.selectView(button.dataset.view || "dashboard");
      });
    });
  }

  bootstrap() {
    const session = this.sessionService.get();
    if (session) {
      this.applySession(session);
    } else {
      this.applyNoSession();
      this.sessionOut.textContent = JSON.stringify(
        { mensaje: "Ingresa como usuario de prueba para habilitar el workspace." },
        null,
        2
      );
    }

    this.selectView("dashboard");
  }

  login() {
    const nombre = this.nameInput.value.trim();
    const rol = this.roleInput.value;
    if (!nombre) {
      this.sessionOut.textContent = JSON.stringify({ error: "El nombre es requerido." }, null, 2);
      return;
    }

    const session = { nombre, rol, ingresoUtc: new Date().toISOString() };
    this.sessionService.save(session);
    this.applySession(session);
    this.sessionOut.textContent = JSON.stringify({ mensaje: "Sesion iniciada", session }, null, 2);

    if (typeof this.onSessionChanged === "function") {
      this.onSessionChanged(true);
    }
  }

  logout() {
    this.sessionService.clear();
    this.applyNoSession();
    this.sessionOut.textContent = JSON.stringify({ mensaje: "Sesion cerrada" }, null, 2);

    if (typeof this.onSessionChanged === "function") {
      this.onSessionChanged(false);
    }

    this.selectView("dashboard");
  }

  applySession(session) {
    this.userChip.textContent = `${session.nombre} (${session.rol})`;
  }

  applyNoSession() {
    this.userChip.textContent = "Invitado";
  }

  selectView(viewName) {
    Object.entries(this.views).forEach(([name, element]) => {
      element.classList.toggle("hidden", name !== viewName);
    });

    this.menuButtons.forEach((button) => {
      button.classList.toggle("active", button.dataset.view === viewName);
    });
  }

  hasSession() {
    return !!this.sessionService.get();
  }
}

class DashboardController {
  constructor(publicoService) {
    this.publicoService = publicoService;
    this.healthBox = document.getElementById("health");
    this.resumenOut = document.getElementById("resumenOut");
  }

  wireEvents() {
    document.getElementById("btnResumen").addEventListener("click", () => this.cargarResumen());
  }

  async cargarHealth() {
    try {
      const data = await this.publicoService.health();
      this.healthBox.innerHTML = `<span class='pill ok'>API OK</span> ${JSON.stringify(data)}`;
    } catch (error) {
      this.healthBox.innerHTML = `<span class='pill bad'>API no disponible</span> ${String(error)}`;
    }
  }

  async cargarResumen() {
    try {
      const data = await this.publicoService.resumen();
      this.resumenOut.textContent = JSON.stringify(data, null, 2);
    } catch (error) {
      this.resumenOut.textContent = String(error);
    }
  }
}

class SedesCrudController {
  constructor(sedesService) {
    this.sedesService = sedesService;
    this.output = document.getElementById("sedesOut");
    this.tableBody = document.getElementById("sedesTableBody");

    this.idInput = document.getElementById("sedeId");
    this.nombreInput = document.getElementById("sedeNombre");
    this.ciudadInput = document.getElementById("sedeCiudad");
    this.activaInput = document.getElementById("sedeActiva");

    this.buttons = {
      crear: document.getElementById("btnCrearSede"),
      listar: document.getElementById("btnListarSedes"),
      buscar: document.getElementById("btnBuscarSede"),
      actualizar: document.getElementById("btnActualizarSede"),
      eliminar: document.getElementById("btnEliminarSede")
    };
  }

  wireEvents() {
    this.buttons.listar.addEventListener("click", () => this.listar());
    this.buttons.buscar.addEventListener("click", () => this.buscarPorId());
    this.buttons.crear.addEventListener("click", () => this.crear());
    this.buttons.actualizar.addEventListener("click", () => this.actualizar());
    this.buttons.eliminar.addEventListener("click", () => this.eliminar());
  }

  setEnabled(enabled) {
    Object.values(this.buttons).forEach((button) => {
      button.disabled = !enabled;
    });
  }

  async listar() {
    try {
      const items = await this.sedesService.listar();
      this.writeOutput(items);
      this.renderTable(items);
    } catch (error) {
      this.writeError(error);
    }
  }

  async buscarPorId() {
    const id = this.readId();
    if (!id) {
      this.writeOutput({ error: "Debes indicar un Id valido." });
      return;
    }

    try {
      const sede = await this.sedesService.obtener(id);
      this.writeOutput(sede);
      this.renderTable([sede]);
      this.syncFormWithSede(sede);
    } catch (error) {
      this.writeError(error);
    }
  }

  async crear() {
    const payload = this.readPayload(false);
    if (!payload) {
      return;
    }

    try {
      const created = await this.sedesService.crear(payload);
      this.writeOutput({ accion: "creada", data: created });
      this.syncFormWithSede(created);
      await this.listar();
    } catch (error) {
      this.writeError(error);
    }
  }

  async actualizar() {
    const id = this.readId();
    if (!id) {
      this.writeOutput({ error: "Debes indicar un Id para actualizar." });
      return;
    }

    const payload = this.readPayload(true);
    if (!payload) {
      return;
    }

    try {
      const updated = await this.sedesService.actualizar(id, payload);
      this.writeOutput({ accion: "actualizada", data: updated });
      await this.listar();
    } catch (error) {
      this.writeError(error);
    }
  }

  async eliminar() {
    const id = this.readId();
    if (!id) {
      this.writeOutput({ error: "Debes indicar un Id para eliminar." });
      return;
    }

    try {
      await this.sedesService.eliminar(id);
      this.writeOutput({ accion: "eliminada", id });
      await this.listar();
    } catch (error) {
      this.writeError(error);
    }
  }

  readId() {
    const value = Number(this.idInput.value.trim());
    return Number.isInteger(value) && value > 0 ? value : null;
  }

  readPayload(includeActive) {
    const nombre = this.nombreInput.value.trim();
    const ciudad = this.ciudadInput.value.trim();

    if (!nombre || !ciudad) {
      this.writeOutput({ error: "Nombre y ciudad son requeridos." });
      return null;
    }

    const payload = { nombre, ciudad };
    if (includeActive) {
      payload.activa = this.activaInput.value === "true";
    }

    return payload;
  }

  syncFormWithSede(sede) {
    this.idInput.value = String(sede.id ?? "");
    this.nombreInput.value = sede.nombre ?? "";
    this.ciudadInput.value = sede.ciudad ?? "";
    this.activaInput.value = String(Boolean(sede.activa));
  }

  renderTable(items) {
    if (!Array.isArray(items) || items.length === 0) {
      this.tableBody.innerHTML = "<tr><td colspan='4'>Sin datos.</td></tr>";
      return;
    }

    this.tableBody.innerHTML = items
      .map((item) =>
        `<tr><td>${item.id}</td><td>${item.nombre}</td><td>${item.ciudad}</td><td>${item.activa}</td></tr>`)
      .join("");
  }

  writeOutput(value) {
    this.output.textContent = JSON.stringify(value, null, 2);
  }

  writeError(error) {
    this.output.textContent = String(error);
  }
}

class CatalogosController {
  constructor(catalogosService) {
    this.catalogosService = catalogosService;
    this.tipoInput = document.getElementById("catalogoTipo");
    this.nombreInput = document.getElementById("catalogoNombre");
    this.output = document.getElementById("catalogosOut");

    this.buttons = {
      listar: document.getElementById("btnListarCatalogo"),
      crear: document.getElementById("btnCrearCatalogo")
    };
  }

  wireEvents() {
    this.buttons.listar.addEventListener("click", () => this.listar());
    this.buttons.crear.addEventListener("click", () => this.crear());
  }

  setEnabled(enabled) {
    Object.values(this.buttons).forEach((button) => {
      button.disabled = !enabled;
    });
  }

  async listar() {
    try {
      const data = await this.catalogosService.listar(this.tipoInput.value);
      this.output.textContent = JSON.stringify(data, null, 2);
    } catch (error) {
      this.output.textContent = String(error);
    }
  }

  async crear() {
    const nombre = this.nombreInput.value.trim();
    if (!nombre) {
      this.output.textContent = JSON.stringify({ error: "Nombre requerido" }, null, 2);
      return;
    }

    try {
      const data = await this.catalogosService.crear(this.tipoInput.value, { nombre });
      this.output.textContent = JSON.stringify({ accion: "creado", data }, null, 2);
      await this.listar();
    } catch (error) {
      this.output.textContent = String(error);
    }
  }
}

(function bootstrap() {
  const apiClient = new ApiClient("http://localhost:8080");

  const sessionService = new SessionService();
  const shell = new PlatformShellController(sessionService);

  const dashboard = new DashboardController(new PublicoService(apiClient));
  const sedesCrud = new SedesCrudController(new SedesService(apiClient));
  const catalogos = new CatalogosController(new CatalogosService(apiClient));

  shell.wireEvents();
  shell.bootstrap();

  dashboard.wireEvents();
  sedesCrud.wireEvents();
  catalogos.wireEvents();

  const applyWorkspaceState = (enabled) => {
    sedesCrud.setEnabled(enabled);
    catalogos.setEnabled(enabled);
  };

  applyWorkspaceState(shell.hasSession());
  shell.onSessionChanged = applyWorkspaceState;

  dashboard.cargarHealth();
  if (shell.hasSession()) {
    sedesCrud.listar();
  }
})();

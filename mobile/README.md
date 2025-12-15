# MiRoti Mobile Integration Guide

Esta guia resume como conectar la app movil Android nativa con la API ya existente en `MiRoti`.

## 1. Panorama general

- **Base URL**: usa `http://10.0.2.2:5000` para emulador Android, `http://<ip-maquina>:5000` en red local o `https://<dominio>:5001` si configuras SSL.
- **Endpoints clave**
  * `POST /api/auth/login` => recibe email y contrasenia, devuelve `token`, `id`, `email`, `rol`.
  * `POST /api/auth/register` => crea cliente y devuelve el mismo payload que login.
  * `GET /api/platos` y `GET /api/platos/{id}` => platos disponibles (rol Cliente o Cadete).
  * `POST /api/pedidos` => crea pedido con lista de detalles.
  * `GET /api/pedidos/mis-pedidos` => pedidos del cliente.
  * `GET /api/pedidos/asignados` y `PUT /api/pedidos/{id}/entregar` => flujo de cadete.

## 2. Configuracion de Gradle

En `module/build.gradle` agrega dependencias basicas:

```gradle
dependencies {
    implementation 'com.squareup.retrofit2:retrofit:2.9.0'
    implementation 'com.squareup.retrofit2:converter-gson:2.9.0'
    implementation 'com.squareup.okhttp3:logging-interceptor:4.11.0'
    implementation 'androidx.lifecycle:lifecycle-livedata-ktx:2.6.1'
    implementation 'androidx.lifecycle:lifecycle-viewmodel:2.6.1'
}
```

Define el host en `build.gradle` para poder cambiarlo segun entorno:

```gradle
buildConfigField "String", "MIROTI_API_BASE", "\"http://10.0.2.2:5000/api/\""
```

Ademas:
* solicita permiso `android.permission.INTERNET`.
* activa `usesCleartextTraffic="true"` si pruebas sin HTTPS.

## 3. Modelos basicos

```java
public class LoginRequest {
    public String email;
    public String contrasenia;
}

public class LoginResponse {
    public String token;
    public int id;
    public String email;
    public String rol;
}

public class PlatoDto {
    public int id;
    public String nombre;
    public String descripcion;
    public BigDecimal precio;
    public String imagenUrl;
}

public class DetallePedidoDto {
    public int platoId;
    public int cantidad;
    public BigDecimal subtotal;
}

public class PedidoCreateDto {
    public BigDecimal total;
    public List<DetallePedidoDto> detalles;
}

public class PedidoDto {
    public int id;
    public String cliente;
    public String cadete;
    public String estado;
    public BigDecimal total;
    public List<DetallePedidoInfoDto> detalles;
}

public class DetallePedidoInfoDto {
    public String plato;
    public int cantidad;
    public BigDecimal subtotal;
}
```

Todos los modelos pueden mapearse con Gson.

## 4. Interfaces Retrofit

```java
public interface AuthApi {
    @POST("auth/login")
    Call<LoginResponse> login(@Body LoginRequest request);

    @POST("auth/register")
    Call<LoginResponse> register(@Body LoginRequest request);
}

public interface PlatosApi {
    @GET("platos")
    Call<List<PlatoDto>> getPlatos();

    @GET("platos/{id}")
    Call<PlatoDto> getPlato(@Path("id") int id);
}

public interface PedidosApi {
    @POST("pedidos")
    Call<PedidoDto> crearPedido(@Body PedidoCreateDto pedido);

    @GET("pedidos/mis-pedidos")
    Call<List<PedidoDto>> getMisPedidos();

    @GET("pedidos/asignados")
    Call<List<PedidoDto>> getPedidosAsignados();

    @PUT("pedidos/{id}/entregar")
    Call<Void> marcarEntregado(@Path("id") int id);
}
```

## 5. Retrofit con token

El token JWT debe enviarse en cada header `Authorization: Bearer <token>`. Crea un interceptor:

```java
public class AuthInterceptor implements Interceptor {
    private final SharedPreferences prefs;

    public AuthInterceptor(Context context) {
        prefs = context.getSharedPreferences("miroti_prefs", Context.MODE_PRIVATE);
    }

    @Override
    public Response intercept(Chain chain) throws IOException {
        String token = prefs.getString("token", null);
        Request request = chain.request();
        if (token != null) {
            request = request.newBuilder()
                    .addHeader("Authorization", "Bearer " + token)
                    .build();
        }
        return chain.proceed(request);
    }
}
```

Construye el cliente Retrofit usando `BuildConfig.MIROTI_API_BASE` y el interceptor:

```java
public class ApiClient {
    private static Retrofit retrofit;

    public static Retrofit getInstance(Context context) {
        if (retrofit == null) {
            OkHttpClient client = new OkHttpClient.Builder()
                    .addInterceptor(new AuthInterceptor(context))
                    .addInterceptor(new HttpLoggingInterceptor().setLevel(Level.BODY))
                    .build();

            retrofit = new Retrofit.Builder()
                    .baseUrl(BuildConfig.MIROTI_API_BASE)
                    .client(client)
                    .addConverterFactory(GsonConverterFactory.create())
                    .build();
        }
        return retrofit;
    }
}
```

## 6. Manejo de autenticacion

Al hacer login:
* invoca `AuthApi.login`.
* si viene `token`, guardalo en `SharedPreferences` (`putString("token", token)`).
* tambien guarda `id`, `rol`, `email`.

Ejemplo de ViewModel (LiveData):

```java
public class AuthViewModel extends ViewModel {
    private final MutableLiveData<Boolean> loginSuccess = new MutableLiveData<>();
    private final AuthApi authApi;

    public AuthViewModel(AuthApi authApi) {
        this.authApi = authApi;
    }

    public LiveData<Boolean> observeLogin() {
        return loginSuccess;
    }

    public void login(String email, String password) {
        LoginRequest request = new LoginRequest();
        request.email = email;
        request.contrasenia = password;

        authApi.login(request).enqueue(new Callback<LoginResponse>() {
            @Override
            public void onResponse(Call<LoginResponse> call, Response<LoginResponse> response) {
                if (response.isSuccessful() && response.body() != null) {
                    saveToken(response.body());
                    loginSuccess.postValue(true);
                } else {
                    loginSuccess.postValue(false);
                }
            }

            @Override
            public void onFailure(Call<LoginResponse> call, Throwable t) {
                loginSuccess.postValue(false);
            }
        });
    }

    private void saveToken(LoginResponse body) {
        SharedPreferences prefs = getApplication().getSharedPreferences("miroti_prefs", Context.MODE_PRIVATE);
        prefs.edit()
            .putString("token", body.token)
            .putInt("userId", body.id)
            .putString("userRole", body.rol)
            .apply();
    }
}
```

Después de `login`, la app puede cargar el menu con `PlatosApi`.

## 7. Flujo del menu

ViewModel para platos:

```java
public class PlatoViewModel extends ViewModel {
    private final PlatosApi api;
    private final MutableLiveData<List<PlatoDto>> platos = new MutableLiveData<>();

    public PlatoViewModel(PlatosApi api) {
        this.api = api;
    }

    public LiveData<List<PlatoDto>> getPlatos() {
        return platos;
    }

    public void fetchPlatos() {
        api.getPlatos().enqueue(new Callback<List<PlatoDto>>() {
            @Override
            public void onResponse(Call<List<PlatoDto>> call, Response<List<PlatoDto>> response) {
                if (response.isSuccessful()) {
                    platos.postValue(response.body());
                }
            }

            @Override
            public void onFailure(Call<List<PlatoDto>> call, Throwable t) {
                platos.postValue(Collections.emptyList());
            }
        });
    }
}
```

## 8. Crear pedido

```java
PedidoCreateDto pedido = new PedidoCreateDto();
pedido.total = subtotal.add(envio);
pedido.detalles = selectedItems.stream()
    .map(plato -> {
        DetallePedidoDto detalle = new DetallePedidoDto();
        detalle.platoId = plato.id;
        detalle.cantidad = plato.quantity;
        detalle.subtotal = plato.precio.multiply(new BigDecimal(plato.quantity));
        return detalle;
    })
    .collect(Collectors.toList());

pedidosApi.crearPedido(pedido).enqueue(new Callback<PedidoDto>() {
    @Override
    public void onResponse(Call<PedidoDto> call, Response<PedidoDto> response) {
        if (response.isSuccessful()) {
            // notificar confirmacion
        }
    }
    ...
});
```

El API devolvera el `PedidoDto` con detalles. Guardalo para mostrar seguimiento.

## 9. Funciones del cadete

* `GET /api/pedidos/asignados` recupera solo los pedidos asignados a quien hizo login.
* `PUT /api/pedidos/{id}/entregar` marca el pedido como entregado y actualiza estado.
* Ambas rutas requieren `Authorization` y rol `Cadete`.

Usa el mismo `ApiClient`, simplemente le cambias la interfaz.

## 10. Consideraciones

- Controla errores 401 para limpiar token y reenviar al login.
- Recuerda que la API devuelve `precio` como decimal; mapealo con `BigDecimal` o `double`.
- Para seguir pedidos usa `PedidoDto.detalles` y muestra `estado` y `fechaHora`.
- Puedes usar `LiveData` + `ViewBinding` para actualizar el UI sin activities a mano.

Si necesitas pantallas, puedes replicar la estetica del prototipo usando `ConstraintLayout`, `CardView` y `RecyclerView` para el menu y los pedidos.

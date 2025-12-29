package com.example.moviespot.e2e

import androidx.activity.ComponentActivity
import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.test.ext.junit.runners.AndroidJUnit4
import com.example.moviespot.di.FakeDiModule
import com.example.moviespot.presentation.screens.movie.MovieViewModel
import com.example.moviespot.presentation.screens.movie.movie_list.MovieFilterScreen
import org.junit.*
import org.junit.runner.RunWith
import okhttp3.mockwebserver.MockWebServer


@RunWith(AndroidJUnit4::class)
class MovieFilterE2ETest {

    @get:Rule
    val composeRule = createAndroidComposeRule<ComponentActivity>()

    private lateinit var server: MockWebServer

    @Before
    fun setup() {
        server = MockWebServer()
        server.start()

        // --- ESTA LINHA É CRUCIAL ---
        // Inicializa os repositórios (incluindo movieRepository)
        FakeDiModule.init(server.url("/").toString())
    }

    @After
    fun teardown() {
        server.shutdown()
    }

    @Test
    fun filter_logic_selection_and_reset() {
        // Dados estáticos para o teste
        val genres = listOf("Action", "Comedy", "Drama")
        val countries = listOf("USA", "UK", "PT")
        val years = listOf("2022", "2023", "2024")

        // Agora FakeDiModule.movieRepository já está inicializado graças ao @Before
        val vm = MovieViewModel(FakeDiModule.movieRepository)

        var applyCalled = false

        composeRule.setContent {
            MovieFilterScreen(
                availableGenres = genres,
                availableCountries = countries,
                availableYears = years,
                selectedGenres = vm.selectedGenres,
                selectedCountries = vm.selectedCountries,
                selectedYears = vm.selectedYears,
                onApplyFilters = { applyCalled = true },
                onBack = {}
            )
        }

        // 1. Testar Seleção Múltipla (Géneros)
        composeRule.onNodeWithText("Action").performClick()
        composeRule.onNodeWithText("Drama").performClick()

        // Verificar no estado do VM
        Assert.assertTrue(vm.selectedGenres.contains("Action"))
        Assert.assertTrue(vm.selectedGenres.contains("Drama"))
        Assert.assertFalse(vm.selectedGenres.contains("Comedy"))

        // Desmarcar "Action"
        composeRule.onNodeWithText("Action").performClick()
        Assert.assertFalse(vm.selectedGenres.contains("Action"))

        // 2. Testar Seleção Única (Anos)
        composeRule.onNodeWithText("2022").performClick()
        Assert.assertTrue(vm.selectedYears.contains("2022"))

        // Clicar noutro ano deve limpar o anterior
        composeRule.onNodeWithText("2024").performClick()
        Assert.assertFalse(vm.selectedYears.contains("2022"))
        Assert.assertTrue(vm.selectedYears.contains("2024"))
        Assert.assertEquals(1, vm.selectedYears.size)

        // 3. Testar Botão Reset
        composeRule.onNodeWithText("Reset").performClick()

        Assert.assertTrue("Géneros devem estar vazios após reset", vm.selectedGenres.isEmpty())
        Assert.assertTrue("Anos devem estar vazios após reset", vm.selectedYears.isEmpty())

        // 4. Testar Submit
        composeRule.onNodeWithText("PT").performClick()
        composeRule.onNodeWithText("Submit").performClick()

        Assert.assertTrue("Callback onApplyFilters deve ser chamada", applyCalled)
        Assert.assertTrue(vm.selectedCountries.contains("PT"))
    }

    @Test
    fun filter_screen_elements_displayed() {
        composeRule.setContent {
            MovieFilterScreen(
                availableGenres = listOf("Genre1"),
                availableCountries = listOf("Country1"),
                availableYears = listOf("2020"),
                selectedGenres = mutableListOf(),
                selectedCountries = mutableListOf(),
                selectedYears = mutableListOf(),
                onApplyFilters = {},
                onBack = {}
            )
        }

        // Verificar se os títulos das secções aparecem
        composeRule.onNodeWithText("Filter").assertIsDisplayed()
        composeRule.onNodeWithText("Genres").assertIsDisplayed()
        composeRule.onNodeWithText("Countries").assertIsDisplayed()
        composeRule.onNodeWithText("Release Year").assertIsDisplayed()

        // Verificar se os botões de ação aparecem
        composeRule.onNodeWithText("Submit").assertIsDisplayed()
        composeRule.onNodeWithText("Reset").assertIsDisplayed()
    }
}
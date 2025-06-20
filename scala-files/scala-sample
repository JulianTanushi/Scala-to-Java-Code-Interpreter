object HelloWorldApp {
  // Case class for a Person
  case class Person(name: String, age: Int)

  // Function to filter adults from a list of people
  def getAdults(people: List[Person]): List[Person] = {
    people.filter(_.age >= 18)
  }

  // Main method
  def main(args: Array[String]): Unit = {
    val people = List(
      Person("Alice", 25),
      Person("Bob", 17),
      Person("Charlie", 30)
    )

    val adults = getAdults(people)

    println("Adults in the list:")
    adults.foreach(person => println(s"${person.name}, ${person.age}"))
  }
}
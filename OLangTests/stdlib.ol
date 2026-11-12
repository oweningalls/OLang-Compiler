enum Maybe<T> {
    Val(T),
    None
}

class StringList {
    [string] arr;
    int length;
    int capacity;
    
    StringList New() {
        return new StringList {
            arr = new [string; 0],
            length = 0,
            capacity = 0
        };
    }
    
    void Add(self, string value) {
        if self.length < self.capacity {
            self.arr[self.length] = value;
            self.length += 1;
            
            return;
        }
        
        if self.capacity == 0 { self.capacity = 1; } else { self.capacity *= 2; }
        let newArr = new [string; self.capacity];
        for i in 0..self.length {
            newArr[i] = self.arr[i];
        }
        
        self.arr = newArr;
        self.arr[self.length] = value;
        self.length += 1;
    }
    
    Maybe<string> Get(self, int idx) {
        if idx < self.length {
            return new Maybe<string>::Val(self.arr[idx]);
        }
        
        return new Maybe<string>::None;
    }
}

class Testing {
    void Assert(bool test, string message) {
        if !test {
            Util::Crash(message);
        }
    }
}

class Util {
    void Crash(string message) {
        print message;
        exit(-1);
    }
    
    void PrintLn(string str) {
        print str + "\n";
    }
}

class MaybeStringUtil {
    bool IsNone(Maybe<string> maybe) {
        return match maybe {
            Maybe<string>::Val(a) => false,
            Maybe<string>::None => true
        };
    }
    
    string GetValueOrDefault(Maybe<string> maybe, string default) {
        return match maybe {
            Maybe<string>::Val(a) => a,
            Maybe<string>::None => default
        };
    }
}

class Program {
    void Main() {
        StringListTests();
    }
    
    void StringListTests() {
        let list = StringList::New();
        Testing::Assert(MaybeStringUtil::IsNone(list.Get(0)), "List shouldn't contain anything yet.");
        list.Add("first");
        let second = "second";
        list.Add(second);
        list.Add("third");
        
        Testing::Assert(MaybeStringUtil::GetValueOrDefault(list.Get(1), "some nonsense") == second, "Indexing was wrong.");
        for i in 0..10 {
            list.Add("");
        }
        
        Testing::Assert(MaybeStringUtil::GetValueOrDefault(list.Get(1), "some nonsense") == second, "Something probably got weird after list reallocation");
    }
}